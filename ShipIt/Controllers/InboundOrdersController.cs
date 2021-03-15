﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;

namespace ShipIt.Controllers
{
    [Route("orders/inbound")]
    public class InboundOrderController : ControllerBase
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockRepository _stockRepository;

        public InboundOrderController(IEmployeeRepository employeeRepository, ICompanyRepository companyRepository, IProductRepository productRepository, IStockRepository stockRepository)
        {
            _employeeRepository = employeeRepository;
            _stockRepository = stockRepository;
            _companyRepository = companyRepository;
            _productRepository = productRepository;
        }

        [HttpGet("{warehouseId}")]
        public InboundOrderResponse Get([FromRoute] int warehouseId)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            Log.Info("orderIn for warehouseId: " + warehouseId);
            // find out the operations manager for the warehouse
            var operationsManager = new Employee(_employeeRepository.GetOperationsManager(warehouseId));

            Log.Debug(String.Format("Found operations manager: {0}", operationsManager));
            // allStock is every line of stock table for the warehouse

            // select stock  and join with product where stock.pr_id=product.pr_id
            // where where warehouse is "", stock.held < productCompany.LowerThreshold && productCompany.Discontinued!=0

            var stockProduct = _stockRepository.GetStockProductByWarehouseId(warehouseId);
            //make a list of unique gcp from stockProduct
            List<String> companyGcps = new List<String>();
            foreach( var sp in stockProduct){
                if(!companyGcps.Contains(sp.Gcp))
                {
                    companyGcps.Add(sp.Gcp);
                }
            }
            List<Company> companyList = new List<Company>();
            foreach(var comGcp in companyGcps)
            {
                Company companyP = new Company(_companyRepository.GetCompany(comGcp));
                companyList.Add(companyP);
            }
 
            Dictionary<Company, List<InboundOrderLine>> orderlinesByCompany = new Dictionary<Company, List<InboundOrderLine>>();
            foreach (var stockPr in stockProduct)
            {
                //go thru the stock list
                // find product details from gtin table using product id
                ////    Company company = new Company(_companyRepository.GetCompany(stockPr.Gcp));
                Company company = companyList.Find(x=>x.Gcp == stockPr.Gcp);
                //select company details from my list and store it in company where gcp = stoctPr.gcp
                // ProductCompanyDataModel productCompany = new ProductCompanyDataModel(_productRepository.GetProductCompanyById(stock.ProductId));
                // compare the stock held in stock table with the threshold in product table. if stock is low,
                // we need to add it to the the order list
               // if(stockPr.held < productCompany.LowerThreshold && productCompany.Discontinued!=0)
                // {
                    // find company details of the product from the gcp table
                  //  // Company company = new Company(productCompany);
                    // determine order quantity
                    var orderQuantity = Math.Max(stockPr.LowerThreshold * 3 - stockPr.held, stockPr.MinimumOrderQuantity);
                    // if company not present, add company.
                    if (!orderlinesByCompany.ContainsKey(company))
                    {
                        orderlinesByCompany.Add(company, new List<InboundOrderLine>());
                    }
                    // add order details to the list
                    orderlinesByCompany[company].Add( 
                        new InboundOrderLine()
                        {
                            gtin = stockPr.Gtin,
                            name = company.Name,
                            quantity = orderQuantity
                        });
                // }
            }

            Log.Debug(String.Format("Constructed order lines: {0}", orderlinesByCompany));

            var orderSegments = orderlinesByCompany.Select(ol => new OrderSegment()
            {
                OrderLines = ol.Value,
                Company = ol.Key
            });

            Log.Info("Constructed inbound order");
            watch.Stop();
            Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");


            return new InboundOrderResponse()
            {
                OperationsManager = operationsManager,
                WarehouseId = warehouseId,
                OrderSegments = orderSegments
            };
            
        }

        [HttpPost("")]
        public void Post([FromBody] InboundManifestRequestModel requestModel)
        {
            Log.Info("Processing manifest: " + requestModel);

            var gtins = new List<string>();

            foreach (var orderLine in requestModel.OrderLines)
            {
                if (gtins.Contains(orderLine.gtin))
                {
                    throw new ValidationException(String.Format("Manifest contains duplicate product gtin: {0}", orderLine.gtin));
                }
                gtins.Add(orderLine.gtin);
            }

            IEnumerable<ProductDataModel> productDataModels = _productRepository.GetProductsByGtin(gtins);
            Dictionary<string, Product> products = productDataModels.ToDictionary(p => p.Gtin, p => new Product(p));

            Log.Debug(String.Format("Retrieved products to verify manifest: {0}", products));

            var lineItems = new List<StockAlteration>();
            var errors = new List<string>();

            foreach (var orderLine in requestModel.OrderLines)
            {
                if (!products.ContainsKey(orderLine.gtin))
                {
                    errors.Add(String.Format("Unknown product gtin: {0}", orderLine.gtin));
                    continue;
                }

                Product product = products[orderLine.gtin];
                if (!product.Gcp.Equals(requestModel.Gcp))
                {
                    errors.Add(String.Format("Manifest GCP ({0}) doesn't match Product GCP ({1})",
                        requestModel.Gcp, product.Gcp));
                }
                else
                {
                    lineItems.Add(new StockAlteration(product.Id, orderLine.quantity));
                }
            }

            if (errors.Count() > 0)
            {
                Log.Debug(String.Format("Found errors with inbound manifest: {0}", errors));
                throw new ValidationException(String.Format("Found inconsistencies in the inbound manifest: {0}", String.Join("; ", errors)));
            }

            Log.Debug(String.Format("Increasing stock levels with manifest: {0}", requestModel));
            _stockRepository.AddStock(requestModel.WarehouseId, lineItems);
            Log.Info("Stock levels increased");
        }
    }
}