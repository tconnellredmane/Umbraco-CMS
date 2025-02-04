using System;
using System.Text;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing
{
    public sealed class UriUtility
    {
        static string _appPath;
        static string _appPathPrefix;

        public UriUtility(IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment is null) throw new ArgumentNullException(nameof(hostingEnvironment));
            ResetAppDomainAppVirtualPath(hostingEnvironment);
        }

        // internal for unit testing only
        internal void SetAppDomainAppVirtualPath(string appPath)
        {
            _appPath = appPath ?? "/";
            _appPathPrefix = _appPath;
            if (_appPathPrefix == "/")
                _appPathPrefix = String.Empty;
        }

        internal void ResetAppDomainAppVirtualPath(IHostingEnvironment hostingEnvironment)
        {
            SetAppDomainAppVirtualPath(hostingEnvironment.ApplicationVirtualPath);
        }

        // will be "/" or "/foo"
        public string AppPath => _appPath;

        // will be "" or "/foo"
        public string AppPathPrefix => _appPathPrefix;

        // adds the virtual directory if any
        // see also VirtualPathUtility.ToAbsolute
        // TODO: Does this do anything differently than IHostingEnvironment.ToAbsolute? Seems it does less, maybe should be removed?
        public string ToAbsolute(string url)
        {
            //return ResolveUrl(url);
            url = url.TrimStart(Constants.CharArrays.Tilde);
            return _appPathPrefix + url;
        }

        // strips the virtual directory if any
        // see also VirtualPathUtility.ToAppRelative
        public string ToAppRelative(string virtualPath)
        {
            if (virtualPath.InvariantStartsWith(_appPathPrefix)
                    && (virtualPath.Length == _appPathPrefix.Length || virtualPath[_appPathPrefix.Length] == '/'))
            {
                virtualPath = virtualPath.Substring(_appPathPrefix.Length);
            }

            if (virtualPath.Length == 0)
            {
                virtualPath = "/";
            }

            return virtualPath;
        }

        // maps an internal umbraco uri to a public uri
        // ie with virtual directory, .aspx if required...
        public Uri UriFromUmbraco(Uri uri, RequestHandlerSettings requestConfig)
        {
            var path = uri.GetSafeAbsolutePath();

            if (path != "/" && requestConfig.AddTrailingSlash)
                path = path.EnsureEndsWith("/");

            path = ToAbsolute(path);

            return uri.Rewrite(path);
        }

        // maps a media umbraco uri to a public uri
        // ie with virtual directory - that is all for media
        public Uri MediaUriFromUmbraco(Uri uri)
        {
            var path = uri.GetSafeAbsolutePath();
            path = ToAbsolute(path);
            return uri.Rewrite(path);
        }

        // maps a public uri to an internal umbraco uri
        // ie no virtual directory, no .aspx, lowercase...
        public Uri UriToUmbraco(Uri uri)
        {
            // TODO: This is critical code that executes on every request, we should
            // look into if all of this is necessary? not really sure we need ToLower?

            // note: no need to decode uri here because we're returning a uri
            // so it will be re-encoded anyway
            var path = uri.GetSafeAbsolutePath();

            path = path.ToLower();
            path = ToAppRelative(path); // strip vdir if any

            if (path != "/")
            {
                path = path.TrimEnd(Constants.CharArrays.ForwardSlash);
            }

            return uri.Rewrite(path);
        }

        #region ResolveUrl

        // http://www.codeproject.com/Articles/53460/ResolveUrl-in-ASP-NET-The-Perfect-Solution
        // note
        // if browsing http://example.com/sub/page1.aspx then
        // ResolveUrl("page2.aspx") returns "/page2.aspx"
        // Page.ResolveUrl("page2.aspx") returns "/sub/page2.aspx" (relative...)
        //
        public string ResolveUrl(string relativeUrl)
        {
            if (relativeUrl == null) throw new ArgumentNullException("relativeUrl");

            if (relativeUrl.Length == 0 || relativeUrl[0] == '/' || relativeUrl[0] == '\\')
                return relativeUrl;

            int idxOfScheme = relativeUrl.IndexOf(@"://", StringComparison.Ordinal);
            if (idxOfScheme != -1)
            {
                int idxOfQM = relativeUrl.IndexOf('?');
                if (idxOfQM == -1 || idxOfQM > idxOfScheme) return relativeUrl;
            }

            StringBuilder sbUrl = new StringBuilder();
            sbUrl.Append(_appPathPrefix);
            if (sbUrl.Length == 0 || sbUrl[sbUrl.Length - 1] != '/') sbUrl.Append('/');

            // found question mark already? query string, do not touch!
            bool foundQM = false;
            bool foundSlash; // the latest char was a slash?
            if (relativeUrl.Length > 1
                && relativeUrl[0] == '~'
                && (relativeUrl[1] == '/' || relativeUrl[1] == '\\'))
            {
                relativeUrl = relativeUrl.Substring(2);
                foundSlash = true;
            }
            else foundSlash = false;
            foreach (char c in relativeUrl)
            {
                if (!foundQM)
                {
                    if (c == '?') foundQM = true;
                    else
                    {
                        if (c == '/' || c == '\\')
                        {
                            if (foundSlash) continue;
                            else
                            {
                                sbUrl.Append('/');
                                foundSlash = true;
                                continue;
                            }
                        }
                        else if (foundSlash) foundSlash = false;
                    }
                }
                sbUrl.Append(c);
            }

            return sbUrl.ToString();
        }

        #endregion


        /// <summary>
        /// Returns an full URL with the host, port, etc...
        /// </summary>
        /// <param name="absolutePath">An absolute path (i.e. starts with a '/' )</param>
        /// <param name="curentRequestUrl"> </param>
        /// <returns></returns>
        /// <remarks>
        /// Based on http://stackoverflow.com/questions/3681052/get-absolute-url-from-relative-path-refactored-method
        /// </remarks>
        internal Uri ToFullUrl(string absolutePath, Uri curentRequestUrl)
        {
            if (string.IsNullOrEmpty(absolutePath))
                throw new ArgumentNullException(nameof(absolutePath));

            if (!absolutePath.StartsWith("/"))
                throw new FormatException("The absolutePath specified does not start with a '/'");

            return new Uri(absolutePath, UriKind.Relative).MakeAbsolute(curentRequestUrl);
        }


    }
}
