using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Model.GroupUpdate;
using VkNet.Enums;

namespace Tochka.Test
{
    internal class VkApiAdapter
    {
        private VkApi _VkApi;
        private bool _VkApi_Init = false;
        /// <summary>
        /// Объект VkAPI
        /// </summary>
        private VkApi VkApi
        {
            get
            {
                if (!_VkApi_Init)
                {
                    this._VkApi = new VkApi();
                    ApiAuthParams authParams = new ApiAuthParams();
                    authParams.AccessToken = "d2ba77ce2ad9f4edcf010c59300d7c4c475326a9c50fd5f4515161b9a8289aac6084ca27507f6bb06cbb6";
                    authParams.ApplicationId = 6797068;
                    this._VkApi.Authorize(authParams);
                    this._VkApi_Init = true;
                }
                return this._VkApi;
            }
        }
        //Регулярное выражение для определения id пользователя в формате id0000000
        private Regex _UserIdMask = new Regex(@"^id\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        //Регулярное выражение для определения id публичной страницы в формате public000000
        private Regex _PublicIdMask = new Regex(@"^public\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        //Регулярное выражение для определения id группы в формате club000000
        private Regex _GroupIdMask = new Regex(@"^club\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        //Регулярное выражение для определения id встречи в формате event000000
        private Regex _EventIdMask = new Regex(@"^event\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        //Регулярное выражение для получений цифровой части ID
        private Regex _idDigit = new Regex(@"\d+$", RegexOptions.Compiled);
        //ID аккаунта, на стене которого будет публиковаться статистика
        private string _MyAccountId = "id121357629";
        /// <summary>
        /// Получаем записи со стены пользователя по UserName
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Post> GetWallPostByUserID(string userName, ulong count)
        {
            //Создаем объект запроса
            WallGetParams wallGetParams = new WallGetParams() { Count = count };
            //В зависимости от вида переданного userName заполняем объект запроса
            if (this._UserIdMask.IsMatch(userName))
            {
                //Если был передан ID пользователя, заполняем ownerID
                wallGetParams.OwnerId = long.Parse(this._idDigit.Match(userName).Value);
            }
            if(this._PublicIdMask.IsMatch(userName) || this._GroupIdMask.IsMatch(userName) || this._EventIdMask.IsMatch(userName))
            {
                //Если был передан идентификатор сообщества, заполняем ownerID со знаком -
                wallGetParams.OwnerId = -1* long.Parse(this._idDigit.Match(userName).Value);
            }
            else
            {
                //Если было передано короткое имя пользователя/сообщества, то ищем объект с таким коротким именем и получаем его ID
                wallGetParams.OwnerId = this.GetIdByDomain(userName);
                wallGetParams.Domain = userName;
            }         
            //Отправляем запрос на получение постов со стены
            WallGetObject result = this.VkApi.Wall.Get(wallGetParams);
            if (result == null) throw new Exception("Wall.Get response is null");
            return result.WallPosts.ToList();
        }
        /// <summary>
        /// Добавляем пост с текстом на стену пользователя
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="text">Текст</param>
        public void AddTextWallPost(string userName, string text)
        {
            //Создаем объект запроса
            WallPostParams wallPostParams = new WallPostParams();
            //Устанавливаем параметры запроса
            wallPostParams.Message = text;
            if (this._UserIdMask.IsMatch(userName))
            {
                wallPostParams.OwnerId = long.Parse(this._idDigit.Match(userName).Value);
            }
            if (this._PublicIdMask.IsMatch(userName) || this._GroupIdMask.IsMatch(userName) || this._EventIdMask.IsMatch(userName))
            {
                //Если был передан идентификатор сообщества, заполняем ownerID со знаком -
                wallPostParams.OwnerId = -1 * long.Parse(this._idDigit.Match(userName).Value);
            }
            this.VkApi.Wall.Post(wallPostParams); 
        }
        /// <summary>
        /// Добавляем пост к себе на стену
        /// </summary>
        /// <param name="text"></param>
        public void AddTextWallPostToMyWall(string text)
        {
            this.AddTextWallPost(this._MyAccountId, text);
        }
        /// <summary>
        /// Получаем ID объекта(страницы/группы/встречи) по короткому имени
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        private long GetIdByDomain(string domain)
        {
            var obj = this.VkApi.Utils.ResolveScreenName(domain);
            if (obj?.Id ==null || obj?.Type ==null) throw new Exception("Page not found");
            if (obj.Type == VkObjectType.User)
            {
                return obj.Id.Value;
            }
            else
            {
                return -1 * obj.Id.Value;
            }
        }
    }
}
