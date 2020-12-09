using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using WebMaterial.DAL.Data;
using WebMaterial.DAL.Models;

namespace WebMaterial.BLL
{
    public class MailService
    {
        public static async Task SendAsync(string message)
        {
            TelegramBotClient client = new TelegramBotClient("1312796762:AAHB-0erveBBjJfOvHfBrCJvAhbNRL8Koew");
            Telegram.Bot.Types.ChatId id = new Telegram.Bot.Types.ChatId(-1001199019089);
            await client.SendTextMessageAsync(id, message);
        }
    }
}
