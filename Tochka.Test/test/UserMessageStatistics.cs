using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.IO;
using VkNet.Model;


namespace Tochka.Test
{
    internal class UserMessageStatistics
    {
        //Адптер для работы с API VK
        private VkApiAdapter _VkApiAdapter = new VkApiAdapter();
        /// <summary>
        /// Метод, вычисляющий статистику букв для строки
        /// </summary>
        /// <param name="text">Строка</param>
        /// <returns></returns>
        private SortedDictionary<char, double> GetTextStatistics(string text)
        {
            if (text == null) throw new ArgumentNullException($"{nameof(text)}");
            //Общее количество символов
            int letterCount = 0;
            SortedDictionary<char, double> result = new SortedDictionary<char, double>();
            letterCount = 0;
            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    letterCount++;
                    char cl = char.ToLower(c);
                    if (result.ContainsKey(cl))
                    {
                        result[cl] += 1;
                    }
                    else
                    {
                        result.Add(cl, 1);
                    }
                }
            }
            List<char> keys = result.Keys.ToList();
            foreach (char key in keys)
            {
                result[key] = Math.Round(result[key] /= letterCount, 4);
            }
            return result;
        }
        public void GetUsersMessageStatistics()
        {
            //Считываем строку из консоли
            string input = Console.ReadLine().ToLower();
            //Если строка непустая, то пытаемся получить статистику для переданного ID и запускаем новую итерацию
            if (!String.IsNullOrEmpty(input))
            {
                try
                {
                    //Получаем посты со страницы с указанным ID
                    List<Post> postsList = this._VkApiAdapter.GetWallPostByUserID(input, 5);
                    //Накапливаем текст для анализа
                    StringBuilder resultStringBuilder = new StringBuilder();
                    foreach (Post post in postsList)
                    {
                        resultStringBuilder.Append(post.Text);
                    }
                    //Получаем статистику по накопленному тексту
                    var letterStatistics = this.GetTextStatistics(resultStringBuilder.ToString());
                    //Формируем результирующую строку
                    string resultString = $"{input}, статистика для последних 5 постов: {JsonConvert.SerializeObject(letterStatistics)}";
                    //Выводим результат в консоль
                    Console.WriteLine(resultString);
                    //Постим результат на стене
                    this._VkApiAdapter.AddTextWallPostToMyWall(resultString);
                }
                catch (Exception e)
                {
                    //если произошла ошибка выводим сообщение в консоль
                    Console.WriteLine(e.Message);
                }
                finally
                {   
                    //Запускаем новую итерацию
                    this.GetUsersMessageStatistics();
                }
            }
        }
    }
}
