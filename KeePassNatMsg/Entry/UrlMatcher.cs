using System;

namespace KeePassNatMsg.Entry
{
    internal static class UrlMatcher
    {
        internal static bool IsMatch(string entryUrl, Uri requestedUrl, UrlMatchMode mode, bool matchScheme)
        {
            Uri candidate;
            if (requestedUrl == null || !Uri.TryCreate(entryUrl, UriKind.Absolute, out candidate))
                return false;

            if ((mode == UrlMatchMode.Exact || matchScheme)
                && !string.Equals(candidate.Scheme, requestedUrl.Scheme, StringComparison.OrdinalIgnoreCase))
                return false;

            switch (mode)
            {
                case UrlMatchMode.Exact:
                    return SameExactAuthority(candidate, requestedUrl)
                        && string.Equals(candidate.PathAndQuery, requestedUrl.PathAndQuery, StringComparison.Ordinal);

                case UrlMatchMode.Hostname:
                    return SameAuthority(candidate, requestedUrl);

                default:
                    return SameDomain(candidate.Host, requestedUrl.Host);
            }
        }

        private static bool SameAuthority(Uri first, Uri second)
        {
            return string.Equals(first.Host, second.Host, StringComparison.OrdinalIgnoreCase)
                && GetExplicitPort(first) == GetExplicitPort(second);
        }

        private static bool SameExactAuthority(Uri first, Uri second)
        {
            return string.Equals(first.Host, second.Host, StringComparison.OrdinalIgnoreCase)
                && first.Port == second.Port;
        }

        private static int GetExplicitPort(Uri uri)
        {
            return uri.IsDefaultPort ? -1 : uri.Port;
        }

        private static bool SameDomain(string firstHost, string secondHost)
        {
            if (string.Equals(firstHost, secondHost, StringComparison.OrdinalIgnoreCase))
                return true;

            if (Uri.CheckHostName(firstHost) != UriHostNameType.Dns
                || Uri.CheckHostName(secondHost) != UriHostNameType.Dns)
                return false;

            firstHost = firstHost.TrimEnd('.');
            secondHost = secondHost.TrimEnd('.');
            return secondHost.EndsWith("." + firstHost, StringComparison.OrdinalIgnoreCase)
                || firstHost.EndsWith("." + secondHost, StringComparison.OrdinalIgnoreCase);
        }
    }
}
