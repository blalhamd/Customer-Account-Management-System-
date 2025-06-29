namespace CAMS.API.Helpers
{
    public static class HttpRequestExtensions
    {
        public static string? GetUserId(this HttpRequest request)
        {
            var token = request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
                return null;

            return JwtHelper.GetUserIdFromToken(token); // your custom helper
        }
    }
}
