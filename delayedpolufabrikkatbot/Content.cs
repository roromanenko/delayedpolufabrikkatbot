﻿using delayedpolufabrikkatbot.Models;

namespace delayedpolufabrikkatbot
{
    public class Content
    {
        public Content()
        {
        }

        public string GetInfo()
        {

            var info = "Ку, фабрикатыч! Этот бот предназначен для пользователей, которые хотят делиться своими идеями, текстами или изображениями и получать репутацию за их одобрение модерацией. Основные функции бота:\n" +
               "1. Вы можете отправить текстовую или медийную публикацию, которая будет переслана на модерацию.\n" +
               "2. За каждую одобренную публикацию вы получаете очки, которые отражают ваш вклад и активность. За репутацию вы можете приобрести различные плюшки;), которые находяться в разделе ''Магазин''\n" +
               "3. В вашем профиле отображается информация о вашей репутации, количестве предложенных и одобренных публикаций.\n" +
               "Внимание! То что вы сделали пост, не означает, что он со 100% вероятностью будет опубликован. Так же ваше авторство может быть не указано, либо публикация переделана, но репутацию при одобрении вы в любом случае получите";

            return info;
        }

        public string GetPostInfo()
        {
            var postInfo = "Отправьте свой пост одним сообщением!\n" +
                "Все публикации, отправляемые пользователями на модерацию, должны соответствовать определенным требованиям.\n" +
                "1. Медийные файлы (изображения и видео) должны быть достаточно высокого качества, обязательно добавление водяного знака с текстом ''тгк:полуфабрикаты'' на все медийные материалы.\n" +
                "2. Текстовые посты не должны быть копиркой чужих статей\n" +
                "При несоблюдённых требованиях высока вероятность, что вашу публикацию проигнорируют";

            return postInfo;
        }

        public string GetProfileInfo(User user, List<PostSubmition> userPosts)
        {
            var profileInfo = "Информация о пользователе:\n" +
               $"ID пользователя: {user.TelegramId}\n" +
               $"Ваши последние 10 публикаций:\n";

            if (userPosts != null && userPosts.Count > 0)
            {
                int i = 1;
                foreach (var post in userPosts)
                {
                    profileInfo += $"{i}. Публикация: {post.PostTitle}\n" +
                                   $"Репутация за публикацию: {post.ChangeReputation}\n\n";
                    i++;
                }
            }
            else
            {
                profileInfo += "У вас пока нет публикаций.\n";
            }

            return profileInfo;
        }
    }
}