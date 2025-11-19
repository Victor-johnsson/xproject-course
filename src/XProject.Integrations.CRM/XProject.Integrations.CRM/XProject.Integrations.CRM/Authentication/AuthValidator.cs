namespace XProject.Integrations.CRM.Authentication
{
    public static class AuthValidator
    {
        public static bool Validate(IHttpContextAccessor httpContentAcc)
        {
            if (httpContentAcc.HttpContext.Request.Headers["api-key"].ToString().Equals("f629dbbe-bd26-4b68-9849-e9ee6970a562"))
            {
                return true;
            }

            return false;
        }
    }
}
