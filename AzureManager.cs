using Bot_Application1.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bot_Application1
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<User> userTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://luke-msabotdb.azurewebsites.net");
            this.userTable = this.client.GetTable<User>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task<List<User>> GetUser(string name)
        {
            //return await this.userTable.Where(user => user.Name == name).ToListAsync();
            List<User> thing = await this.userTable.ToListAsync();
            return thing;
        }

        public async Task AddUser(User user)
        {
            await this.userTable.InsertAsync(user);
        }

        public async Task UpdateUser(User user)
        {
            await this.userTable.UpdateAsync(user);
        }

    }
}