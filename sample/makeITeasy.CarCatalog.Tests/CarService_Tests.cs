﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using makeITeasy.AppFramework.Core.Commands;
using makeITeasy.AppFramework.Core.Interfaces;
using makeITeasy.AppFramework.Models;
using makeITeasy.CarCatalog.Core.Services.Interfaces;
using makeITeasy.CarCatalog.Core.Services.Queries.CarQueries;
using makeITeasy.CarCatalog.Infrastructure.Data;
using makeITeasy.CarCatalog.Models;
using makeITeasy.CarCatalog.Tests.Catalogs;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace makeITeasy.CarCatalog.Tests
{
    public class CarService_Tests : UnitTestAutofacService<ServiceRegistrationAutofacModule>
    {
        private ICarService carService;
        private readonly List<Car> carList;

        public CarService_Tests()
        {
            carService = Resolve<ICarService>();
            var t = Resolve<CarCatalogContext>();

            t.Database.EnsureCreated();

            carList = TestCarsCatalog.GetCars();

            carList.ForEach(async x => await carService.CreateAsync(x));
        }

        ~CarService_Tests()
        {
            carService = null;
        }

        [Fact]
        public void IsValid_InValidObjectTest()
        {
            var newCar = new Car
            {
                Name = "x"
            };

            carService.IsValid(newCar).Should().BeFalse();
        }

        [Fact]
        public void IsValid_ValidObjectTest()
        {
            var newCar = new Car
            {
                Name = "xxxx"
            };

            carService.IsValid(newCar).Should().BeTrue();
        }

        [Fact]
        public async Task Create_InValidObjectTest()
        {
            var newCar = new Car
            {
                Name = "x"
            };

            var result = await carService.CreateAsync(newCar);

            result.Result.Should().Be(CommandState.Error);
        }

        [Fact]
        public void Create_UncompleteObjectTest()
        {
            var newCar = new Car
            {
                Name = "xxx"
            };

            carService.Invoking(y => y.CreateAsync(newCar)).Should().Throw<DbUpdateException>();
        }

        [Fact]
        public async Task CreateAndGet_BasicTest()
        {
            Car newCar = new Car()
            {
                Name = "C3",
                ReleaseYear = 2011,
                Brand = new Brand()
                {
                    Name = "Citroen",
                    Country = new Country()
                    {
                        Name = "France",
                        CountryCode = "FR"
                    }
                }
            };

            var result = await carService.CreateAsync(newCar);

            result.Result.Should().Be(CommandState.Success);

            var getResult = await carService.QueryAsync(new BaseCarQuery() { ID = result.Entity.Id }, includeCount: true);

            getResult.TotalItems.Should().Be(1);
            getResult.Results.Should().NotBeEmpty().And.HaveCount(1);
            getResult.Results.Should().SatisfyRespectively(
                first =>
                {
                    first.Id.Should().BeGreaterThan(0);
                    first.Name.Should().Be(newCar.Name);
                });
        }

        [Fact]
        public async Task CreateAndGet_ListTest()
        {
            var getResult = await carService.QueryAsync(new BaseCarQuery(), includeCount: true);

            getResult.TotalItems.Should().Be(carList.Count);

            getResult.Results.Select(x => x.Id).Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task CreateAndGet_ListWithIncludeStringTest()
        {
            var getResult = await carService.QueryAsync(new BaseCarQuery() { IncludeStrings = new List<string>() { "Brand.Country" } }, includeCount: true);

            getResult.TotalItems.Should().Be(carList.Count);

            getResult.Results.Select(x => x.Id).Should().BeInAscendingOrder();

            getResult.Results.Should().OnlyContain(x => x.Brand.Country != null);
        }

        [Fact]
        public async Task CreateAndGet_ListWithIncludeTest()
        {
            var getResult = await carService.QueryAsync
                (new BaseCarQuery() { Includes = new List<System.Linq.Expressions.Expression<Func<Car, object>>>() { x => x.Brand.Country } }, includeCount: true);

            getResult.TotalItems.Should().Be(carList.Count);

            getResult.Results.Select(x => x.Id).Should().BeInAscendingOrder();

            getResult.Results.Should().OnlyContain(x => x.Brand.Country != null);
        }

        public class SmallCarInfo : IMapFrom<Car>
        {
            public long ID { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public async Task CreateAndGet_ListWithMappingTest()
        {
            var getResult = await carService.QueryWithProjectionAsync<SmallCarInfo>
                (new BaseCarQuery() { }, includeCount: true);

            getResult.TotalItems.Should().Be(carList.Count);

            getResult.Results.Select(x => x.ID).Should().BeInAscendingOrder();

            getResult.Results.Should().OnlyContain(x => x.Name != null);

        }

        [Fact]
        public async Task CreateAndGet_ListWithFunctionTest()
        {
            var getResult = await carService.QueryAsync
                (new BaseCarQuery() { IsModernCar = true }, includeCount: true);

            getResult.TotalItems.Should().Be(carList.Count(x => x.ReleaseYear > 2000));
        }

        public class SmallCarInfoWithBrand : IMapFrom<Car>
        {
            public long ID { get; set; }
            public string Name { get; set; }
            public string BrandName { get; set; } //Automatic mapping with Brand.Name
            public string BrandCountryName { get; set; }
        }

        [Fact]
        public async Task CreateAndGet_ListWithMapping2LevelTest()
        {
            var getResult = await carService.QueryWithProjectionAsync<SmallCarInfoWithBrand>
                (new BaseCarQuery() { }, includeCount: true);

            getResult.TotalItems.Should().Be(carList.Count);

            getResult.Results.Select(x => x.ID).Should().BeInAscendingOrder();

            getResult.Results.Should().OnlyContain(x => x.Name != null);
            getResult.Results.Should().OnlyContain(x => x.BrandName != null);
        }

        [Fact]
        public async Task CreateAndGet_ListWithPagingTest()
        {
            const int pageSize = 10;
            var getResult = await carService.QueryAsync(new BaseCarQuery() { Skip = 5, Take = pageSize, IsPagingEnabled = true }, includeCount: true);

            getResult.TotalItems.Should().Be(carList.Count);
            getResult.Results.Count.Should().Be(Math.Min(carList.Count, pageSize));
        }

        [Fact]
        public async Task OrderString_Tests()
        {
            var getResult = await carService.QueryAsync(new BaseCarQuery() 
            {  
                OrderByStrings = new List<OrderBySpecification<string>>() { new OrderBySpecification<string>() { OrderBy = nameof(Car.ReleaseYear), SortDescending = false } }
            });

            getResult.Results.Select(x => x.ReleaseYear).Should().BeInAscendingOrder();

            getResult = await carService.QueryAsync(new BaseCarQuery()
            {
                OrderByStrings = new List<OrderBySpecification<string>>() { new OrderBySpecification<string>() { OrderBy = nameof(Car.ReleaseYear), SortDescending = true } }
            });

            getResult.Results.Select(x => x.ReleaseYear).Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task OrderWith2LevelString_Test()
        {
            var getResult = await carService.QueryAsync(new BaseCarQuery() { 
                OrderByStrings = new List<OrderBySpecification<string>>() { new OrderBySpecification<string>() { OrderBy = "Brand.Country.Name", SortDescending = false } },
                    IncludeStrings = new List<string> { "Brand.Country" } });

            getResult.Results.Select(x => (int)x.Brand.Country.Name.First()).Should().BeInAscendingOrder();

            getResult = await carService.QueryAsync(new BaseCarQuery()
            {
                OrderByStrings = new List<OrderBySpecification<string>>() { new OrderBySpecification<string>() { OrderBy = "Brand.Country.Name", SortDescending = true } },
                IncludeStrings = new List<string> { "Brand.Country" }
            });

            getResult.Results.Select(x => (int)x.Brand.Country.Name.First()).Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task OrderFunction_Tests()
        {

            var getResult = await carService.QueryAsync(new BaseCarQuery()
            {
                OrderBy = new List<OrderBySpecification<Expression<Func<Car, object>>>>() { new OrderBySpecification<Expression<Func<Car, object>>>() { OrderBy = x => x.ReleaseYear} }
            });

            getResult.Results.Select(x => x.ReleaseYear).Should().BeInAscendingOrder();


            getResult = await carService.QueryAsync(new BaseCarQuery()
            {
                OrderBy = new List<OrderBySpecification<Expression<Func<Car, object>>>>() { new OrderBySpecification<Expression<Func<Car, object>>>() { OrderBy = x => x.ReleaseYear, SortDescending = true } }
            });

            getResult.Results.Select(x => x.ReleaseYear).Should().BeInDescendingOrder();
        }


        [Fact]
        public async Task OrderFunction2Level_Tests()
        {
            Car newCar = new Car()
            {
                Name = "C3",
                ReleaseYear = 2011,
                Brand = new Brand()
                {
                    Name = "Citroen",
                    Country = new Country()
                    {
                        Name = "France",
                        CountryCode = "FR"
                    }
                }
            };

            _ = await carService.CreateAsync(newCar);

            Car newCar2 = new Car()
            {
                Name = "B2",
                ReleaseYear = 2011,
                Brand = new Brand()
                {
                    Name = "Citroen",
                    Country = new Country()
                    {
                        Name = "France",
                        CountryCode = "FR"
                    }
                }
            };

            _ = await carService.CreateAsync(newCar2);

            Car newCar3 = new Car()
            {
                Name = "A2",
                ReleaseYear = 2011,
                Brand = new Brand()
                {
                    Name = "Citroen",
                    Country = new Country()
                    {
                        Name = "France",
                        CountryCode = "FR"
                    }
                }
            };

            _ = await carService.CreateAsync(newCar3);

            var getResult = await carService.QueryAsync(new BaseCarQuery()
            {
                OrderBy = new List<OrderBySpecification<Expression<Func<Car, object>>>>() { 
                    new OrderBySpecification<Expression<Func<Car, object>>>() { OrderBy = x => x.ReleaseYear } ,
                    new OrderBySpecification<Expression<Func<Car, object>>>() { OrderBy = x => x.Name }
                }
            });

            getResult.Results.Select(x => x.ReleaseYear).Should().BeInAscendingOrder();
            getResult.Results.Where(x => x.ReleaseYear == newCar.ReleaseYear).Select(x => x.Name).Should().BeInAscendingOrder();

            BaseCarQuery specification = new BaseCarQuery() { };
            specification.AddOrder(x => x.ReleaseYear, false);
            specification.AddOrder(x => x.Name, true);
            getResult = await carService.QueryAsync(specification);

            getResult.Results.Select(x => x.ReleaseYear).Should().BeInAscendingOrder();
            getResult.Results.Where(x => x.ReleaseYear == newCar.ReleaseYear).Select(x => x.Name).Should().BeInDescendingOrder();

            getResult = await carService.QueryAsync(new BaseCarQuery()
            {
                OrderBy = new List<OrderBySpecification<Expression<Func<Car, object>>>>() {
                    new OrderBySpecification<Expression<Func<Car, object>>>() { OrderBy = x => x.ReleaseYear } ,
                    new OrderBySpecification<Expression<Func<Car, object>>>() { OrderBy = x => x.Name , SortDescending = true}
                }
            });

            getResult.Results.Select(x => x.ReleaseYear).Should().BeInAscendingOrder();
            getResult.Results.Where(x => x.ReleaseYear == newCar.ReleaseYear).Select(x => x.Name).Should().BeInDescendingOrder();

        }
    }
}
