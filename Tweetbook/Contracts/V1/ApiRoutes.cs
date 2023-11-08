using System.Globalization;

namespace Tweetbook.Contracts.V1
{
    public static class ApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = $"{Root}/{Version}";

        public static class Posts
        {
            public const string GetAll = Base + "/posts";

            public const string Get = Base + "/posts/{postId}";

            public const string Create = Base + "/posts";

            public const string Update = Base + "/posts/{postId}";

            public const string Delete = Base + "/posts/{postId}";
        }

        /// <summary>
        /// This class is used to define the routes for the Identity controller.
        /// It violates the RESTful principles, but it's a common practice to have a separate controller for the identity.
        /// Ideally, it would be part of a separate service.
        /// </summary>
        public static class Identity
        {
            public const string Login = Base + "/identity/login";

            public const string Register = Base + "/identity/register";

            public const string Refresh = Base + "/identity/refresh";
        }

        public static class Tags
        {
            public const string GetAll = Base + "/tags";

            public const string Get = Base + "/tags/{tagName}";

            public const string Create = Base + "/tags";

            public const string Update = Base + "/tags/{tagName}";

            public const string Delete = Base + "/tags/{tagName}";
        }
    }
}
