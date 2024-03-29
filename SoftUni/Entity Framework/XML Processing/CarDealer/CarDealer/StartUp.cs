﻿namespace CarDealer
{
	using System;
	using AutoMapper;
	using AutoMapper.QueryableExtensions;

	using CarDealer.Data;
	using CarDealer.DTOs.Export;
	using CarDealer.DTOs.Import;
	using CarDealer.Models;
	using CarDealer.Utilities;

	public class StartUp
	{
		public static void Main()
		{
			using CarDealerContext context = new CarDealerContext();

			// Importing code:
			// string xml = File.ReadAllText("../../../Datasets/dataSetName.xml");
			// ImportSales(context, xml);

			// Exporting code:
			//Console.WriteLine(MethodName(context));
		}

		public static string ImportSuppliers(CarDealerContext context, string inputXml)
		{
			IMapper mapper = InitializeMapper();
			XmlHelper helper = new XmlHelper();

			SupplierDto[] supplierDtos =
				helper.Deserialize<SupplierDto[]>(inputXml, "Suppliers");

			ICollection<Supplier> validSuppliers = new HashSet<Supplier>();

			foreach (SupplierDto supplierDto in supplierDtos)
			{
				if (!string.IsNullOrEmpty(supplierDto.Name))
				{
					Supplier supplier = mapper.Map<Supplier>(supplierDto);
					validSuppliers.Add(supplier);
				}
			}

			context.Suppliers.AddRange(validSuppliers);
			context.SaveChanges();

			return $"Successfully imported {validSuppliers.Count}";
		}

		public static string ImportParts(CarDealerContext context, string inputXml)
		{
			IMapper mapper = InitializeMapper();
			XmlHelper helper = new XmlHelper();

			PartDto[] partDtos = helper.Deserialize<PartDto[]>(inputXml, "Parts");

			ICollection<Part> validParts = new HashSet<Part>();

			foreach (PartDto partDto in partDtos)
			{
				if (!string.IsNullOrEmpty(partDto.Name) &&
					partDto.SupplierId.HasValue &&
					context.Suppliers.Any(s => s.Id == partDto.SupplierId))
				{
					Part part = mapper.Map<Part>(partDto);
					validParts.Add(part);
				}
			}

			context.Parts.AddRange(validParts);
			context.SaveChanges();

			return $"Successfully imported {validParts.Count}";
		}

		public static string ImportCars(CarDealerContext context, string inputXml)
		{
			IMapper mapper = InitializeMapper();
			XmlHelper helper = new XmlHelper();

			CarDto[] carDtos = helper.Deserialize<CarDto[]>(inputXml, "Cars");

			ICollection<Car> validCars = new HashSet<Car>();

			foreach (CarDto carDto in carDtos)
			{
				if (string.IsNullOrEmpty(carDto.Make) ||
					string.IsNullOrEmpty(carDto.Model))
				{
					continue;
				}

				Car car = mapper.Map<Car>(carDto);

				foreach (CarPartDto partDto in carDto.Parts.DistinctBy(p => p.PartId))
				{
					if (!context.Parts.Any(p => p.Id == partDto.PartId))
					{
						continue;
					}

					PartCar carPart = new()
					{
						PartId = partDto.PartId
					};

					car.PartsCars.Add(carPart);
				}

				validCars.Add(car);
			}

			context.Cars.AddRange(validCars);
			context.SaveChanges();

			return $"Successfully imported {validCars.Count}";
		}

		public static string ImportCustomers(CarDealerContext context, string inputXml)
		{
			IMapper mapper = InitializeMapper();
			XmlHelper helper = new XmlHelper();

			CustomerDto[] customerDtos = helper.Deserialize<CustomerDto[]>(inputXml, "Customers");

			ICollection<Customer> validCustomers = new HashSet<Customer>();

			foreach (CustomerDto customerDto in customerDtos)
			{
				if (string.IsNullOrEmpty(customerDto.Name) ||
					string.IsNullOrEmpty(customerDto.BirthDate))
				{
					continue;
				}

				Customer customer = mapper.Map<Customer>(customerDto);
				validCustomers.Add(customer);
			}

			context.Customers.AddRange(validCustomers);
			context.SaveChanges();

			return $"Successfully imported {validCustomers.Count}";
		}

		public static string ImportSales(CarDealerContext context, string inputXml)
		{
			IMapper mapper = InitializeMapper();
			XmlHelper helper = new XmlHelper();

			SaleDto[] saleDtos = helper.Deserialize<SaleDto[]>(inputXml, "Sales");

			ICollection<Sale> validSales = new HashSet<Sale>();

			foreach (SaleDto saleDto in saleDtos)
			{
				if (context.Cars.Any(c => c.Id == saleDto.CarId))
				{
					Sale sale = mapper.Map<Sale>(saleDto);
					validSales.Add(sale);
				}
			}

			context.Sales.AddRange(validSales);
			context.SaveChanges();

			return $"Successfully imported {validSales.Count}";
		}

		public static string GetCarsWithDistance(CarDealerContext context)
		{
			IMapper mapper = InitializeMapper();
			XmlHelper helper = new XmlHelper();

			ExportCarDto[] cars = context.Cars
				.Where(c => c.TraveledDistance > 2000000)
				.OrderBy(c => c.Make)
				.ThenBy(c => c.Model)
				.Take(10)
				.ProjectTo<ExportCarDto>(mapper.ConfigurationProvider)
				.ToArray();

			return helper.Serialize(cars, "cars");
		}

		public static string GetCarsFromMakeBmw(CarDealerContext context)
		{
			IMapper mapper = InitializeMapper();
			XmlHelper helper = new XmlHelper();

			ExportBmwCarDto[] cars = context.Cars
				.Where(c => c.Make == "BMW")
				.OrderBy(c => c.Model)
				.ThenByDescending(c => c.TraveledDistance)
				.ProjectTo<ExportBmwCarDto>(mapper.ConfigurationProvider)
				.ToArray();

			return helper.Serialize(cars, "cars");
		}

		public static string GetLocalSuppliers(CarDealerContext context)
		{
			IMapper mapper = InitializeMapper();
			XmlHelper helper = new XmlHelper();

			var suppliers = context.Suppliers
				.Where(s => !s.IsImporter)
				.ProjectTo<ExportLocalSupplierDto>(mapper.ConfigurationProvider)
				.ToArray();

			return helper.Serialize(suppliers, "suppliers");
		}

		public static string GetCarsWithTheirListOfParts(CarDealerContext context)
		{
			IMapper mapper = InitializeMapper();
			XmlHelper helper = new XmlHelper();

			ExportCarWithPartsListDto[] cars = context.Cars
				.OrderByDescending(c => c.TraveledDistance)
				.ThenBy(c => c.Model)
				.Take(5)
				.ProjectTo<ExportCarWithPartsListDto>(mapper.ConfigurationProvider)
				.ToArray();

			return helper.Serialize(cars, "cars");
		}

		public static string GetTotalSalesByCustomer(CarDealerContext context)
		{
			IMapper mapper = InitializeMapper();
			XmlHelper helper = new XmlHelper();

			var tempDto = context.Customers
				.Where(c => c.Sales.Any())
				.Select(c => new
				{
					FullName = c.Name,
					BoughtCars = c.Sales.Count(),
					SalesInfo = c.Sales.Select(s => new
					{
						Prices = c.IsYoungDriver
							? s.Car.PartsCars.Sum(p => Math.Round((double)p.Part.Price * 0.95, 2))
							: s.Car.PartsCars.Sum(p => (double)p.Part.Price)
					}).ToArray(),
				})
				.ToArray();

			ExportSaleByCustomerDto[] totalSalesDtos = tempDto
				.OrderByDescending(t => t.SalesInfo.Sum(s => s.Prices))
				.Select(t => new ExportSaleByCustomerDto()
				{
					FullName = t.FullName,
					BoughtCars = t.BoughtCars,
					SpentMoney = decimal.Parse(t.SalesInfo
						.Sum(s => s.Prices)
						.ToString("f2"))
				})
				.ToArray();

			return helper.Serialize(totalSalesDtos, "customers");
		}

		public static string GetSalesWithAppliedDiscount(CarDealerContext context)
		{
			XmlHelper helper = new XmlHelper();

			var sales = context.Sales
				.Select(s => new ExportSaleWithAppliedDiscountDto
				{
					CarDto = new ExportCarWithAppliedDiscountDto
					{
						Make = s.Car.Make,
						Model = s.Car.Model,
						TraveledDistance = s.Car.TraveledDistance,
					},
					Discount = s.Discount,
					CustomerName = s.Customer.Name,
					Price = s.Car.PartsCars
						.Sum(p => p.Part.Price),
					PriceWithDiscount = s.Car.PartsCars
						.Sum(p => p.Part.Price) - s.Car.PartsCars.Sum(p => p.Part.Price) * s.Discount / 100
				})
				.ToArray();

			return helper.Serialize(sales, "sales");
		}

		private static IMapper InitializeMapper()
			=> new Mapper(
				new MapperConfiguration(cfg =>
				{
					cfg.AddProfile<CarDealerProfile>();
				}));
	}
}