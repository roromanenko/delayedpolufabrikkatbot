using Microsoft.AspNetCore.Mvc;

namespace delayedpolufabrikkatbot.Interfaces
{
    public interface ICacheService
    {
        public string GetCache(long telegramId);

        public string SetCache(long telegramId, string caheValue);

        public void ClearCache(long telegramId);
    }
}