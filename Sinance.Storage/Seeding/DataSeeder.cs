using Sinance.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Sinance.Storage.Seeding
{
    public class DataSeeder
    {
        private readonly Random _random = new Random();
        private readonly Func<IUnitOfWork> _unitOfWork;

        public DataSeeder(Func<IUnitOfWork> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SeedData()
        {
            using (var unitOfWork = _unitOfWork())
            {
                var user = await InsertOrGetUser(unitOfWork);

                await InsertTransactionsAndCategories(unitOfWork, user);

                await unitOfWork.SaveAsync();
            }
        }

        private static async Task<SinanceUser> InsertOrGetUser(IUnitOfWork unitOfWork)
        {
            const string demoUserName = "DemoUser";
            var demoUser = unitOfWork.UserRepository.FindSingle(x => x.Username == demoUserName);

            if (demoUser != null)
            {
                demoUser = new SinanceUser
                {
                    Password = "Demo",
                    Username = demoUserName
                };

                unitOfWork.UserRepository.Insert(demoUser);
                await unitOfWork.SaveAsync();
            }

            return demoUser;
        }

        private Category InsertOrGetCategory(IUnitOfWork unitOfWork, SinanceUser demoUser, string categoryName, bool isRegular, Category parentCategory = null)
        {
            var category = unitOfWork.CategoryRepository.FindSingle(x => x.Name == categoryName);

            if (category == null)
            {
                category = new Category
                {
                    Name = categoryName,
                    User = demoUser,
                    ParentCategory = parentCategory,
                    ColorCode = string.Format("#{0:X6}", _random.Next(0x1000000)),
                    IsRegular = isRegular
                };

                unitOfWork.CategoryRepository.Insert(category);
            }

            return category;
        }

        private async Task InsertTransactionsAndCategories(IUnitOfWork unitOfWork, SinanceUser demoUser)
        {
            var essentialsCategory = InsertOrGetCategory(unitOfWork, demoUser, "Essentials", false);

            var foodCategory = InsertOrGetCategory(unitOfWork, demoUser, "Food", false, essentialsCategory);
            var salaryCategory = InsertOrGetCategory(unitOfWork, demoUser, "Salary", true, essentialsCategory);
            var electricityAndGasCategory = InsertOrGetCategory(unitOfWork, demoUser, "Electricity and Gas", true, essentialsCategory);
            var waterCategory = InsertOrGetCategory(unitOfWork, demoUser, "Water", true, essentialsCategory);
            var internetCategory = InsertOrGetCategory(unitOfWork, demoUser, "Internet", true, essentialsCategory);

            var clothesCategory = InsertOrGetCategory(unitOfWork, demoUser, "Clothes", false);
            var electronicsCategory = InsertOrGetCategory(unitOfWork, demoUser, "Electronics", false);

            var hobbyCategory = InsertOrGetCategory(unitOfWork, demoUser, "Hobby", false);
            var gamesCategory = InsertOrGetCategory(unitOfWork, demoUser, "Games", false, hobbyCategory);
            var knittingCategory = InsertOrGetCategory(unitOfWork, demoUser, "Knitting", false, hobbyCategory);

            var subscriptionsCategory = InsertOrGetCategory(unitOfWork, demoUser, "Subscriptions", true);
            var netflixCategory = InsertOrGetCategory(unitOfWork, demoUser, "Netflix", true, subscriptionsCategory);

            var internalCashflowCategory = InsertOrGetCategory(unitOfWork, demoUser, "InternalCashFlow", false);
        }
    }
}