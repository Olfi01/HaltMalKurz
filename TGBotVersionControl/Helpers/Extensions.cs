using HaltMalKurzControl.SQLiteFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HaltMalKurzControl.Helpers
{
    public static class Extensions
    {
        public static string FirstWord(this string str) => str.Contains(" ") ? str.Remove(str.IndexOf(" ")) : str;

        public static bool IsGlobalAdmin(this User user, HaltMalKurzContext db) => user.FindOrCreateBotUser(db).IsGlobalAdmin;

        public static bool TryFind<T>(this IEnumerable<T> list, Func<T, bool> predicate, out T result)
        {
            if (!list.Any(predicate))
            {
                result = default(T);
                return false;
            }
            result = list.First(predicate);
            return true;
        }

        public static bool TryFindWithIndex<T>(this IEnumerable<T> list, Func<T, bool> predicate, out T result, out int index)
        {
            for (int i = 0; i < list.Count(); i++)
            {
                T element = list.ElementAt(i);
                if (predicate.Invoke(element))
                {
                    result = element;
                    index = i;
                    return true;
                }
            }

            result = default(T);
            index = default(int);
            return false;
        }

        public static bool IsGroup(this Chat chat) => chat.Type == ChatType.Group || chat.Type == ChatType.Supergroup;

        public static void Update(this BotUser botUser, User user)
        {
            botUser.FirstName = user.FirstName;
            botUser.LastName = user.LastName;
            botUser.Username = user.Username;
        }

        public static BotUser FindOrCreateBotUser(this User user, HaltMalKurzContext db)
        {
            if (db.Users.Any(x => x.Id == user.Id))
            {
                return db.Users.Find(user.Id);
            }
            else
            {
                var added = db.Users.Add(BotUser.FromUser(user));
                db.SaveChanges();
                return added;
            }
        }

        public static string FullName(this User user)
        {
            return string.Join(" ", user.FirstName, user.LastName).Trim();
        }
    }
}
