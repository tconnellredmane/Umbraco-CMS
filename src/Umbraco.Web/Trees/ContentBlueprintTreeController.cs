using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// The content blueprint tree controller
    /// </summary>
    /// <remarks>
    /// This authorizes based on access to the content section even though it exists in the settings
    /// </remarks>
    [UmbracoApplicationAuthorize(Constants.Applications.Content)]
    [Tree(Constants.Applications.Settings, Constants.Trees.ContentBlueprints, null, sortOrder: 8)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class ContentBlueprintTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            //get all blueprints
            var entities = Services.EntityService.GetChildren(Constants.System.Root, UmbracoObjectTypes.DocumentBlueprint).ToArray();

            //check if we're rendering the root in which case we'll render the content types that have blueprints
            if (id == Constants.System.Root.ToInvariantString())
            {
                //get all blueprint content types
                var contentTypeAliases = entities.Select(x => ((UmbracoEntity) x).ContentTypeAlias).Distinct();
                //get the ids
                var contentTypeIds = Services.ContentTypeService.GetAllContentTypeIds(contentTypeAliases.ToArray());
                //now get the entities ... it's a bit round about but still smaller queries than getting all document types
                var docTypeEntities = Services.EntityService.GetAll(UmbracoObjectTypes.DocumentType, contentTypeIds.ToArray()).ToArray();

                nodes.AddRange(docTypeEntities
                    .Select(entity =>
                    {
                        var treeNode = CreateTreeNode(entity, Constants.ObjectTypes.DocumentBlueprintGuid, id, queryStrings, "icon-item-arrangement", true);
                        treeNode.Path = string.Format("-1,{0}", entity.Id);
                        treeNode.NodeType = "contentType";
                        //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
                        treeNode.AdditionalData["jsClickCallback"] = "javascript:void(0);";
                        return treeNode;
                    }));

                return nodes;
            }
            else
            {
                var intId = id.TryConvertTo<int>();
                //Get the content type
                var ct = Services.ContentTypeService.GetContentType(intId.Result);
                if (ct == null) return nodes;

                var blueprintsForDocType = entities.Where(x => ct.Alias == ((UmbracoEntity) x).ContentTypeAlias);
                nodes.AddRange(blueprintsForDocType
                    .Select(entity =>
                    {
                        var treeNode = CreateTreeNode(entity, Constants.ObjectTypes.DocumentBlueprintGuid, id, queryStrings, "icon-blueprint", false);
                        treeNode.Path = string.Format("-1,{0},{1}", ct.Id, entity.Id);
                        return treeNode;
                    }));
            }

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions                              
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);
                return menu;
            }
            var ct = Services.EntityService.Get(int.Parse(id), UmbracoObjectTypes.DocumentType);
            //only refresh if it's a content type
            if (ct != null)
            {
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);
                return menu;
            }

            menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));


            return menu;
        }
        
    }
}