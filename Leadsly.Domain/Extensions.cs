
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.ViewModels.LinkedInAccount;
using System;
using System.Collections.Generic;

namespace Leadsly.Domain
{
    public static class Extensions
    {
        public static AccountViewModel ToConnectedAccountViewModel(this VirtualAssistant virtualAssistant)
        {
            return new AccountViewModel
            {
                Email = virtualAssistant.SocialAccount.Username
            };
        }

        public static string Capitalize(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }

        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> dictionaries)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            foreach (var dict in dictionaries)
            {
                foreach (var x in dict)
                {
                    result[x.Key] = x.Value;
                }
            }

            return result;
        }

        public static void AddRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Collection is null");
            }

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                {
                    source.Add(item.Key, item.Value);
                }
                else
                {
                    // handle duplicate key issue here
                }
            }
        }
    }
}
