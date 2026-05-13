using Microsoft.AspNetCore.Http;
namespace WRR.Admin.Extension
{
    public static class SessionExtensions
    {
        public static int GetInt(this ISession session, string key)
        {
            var data = session.GetString(key);
            if (data == null)
            {
                return 0;
            }
            return int.Parse(data);
        }

        public static void SetInt(this ISession session, string key, int value)
        {
            session.SetString(key, value.ToString());
        }
    }
}
