#!/usr/bin/env python3

from pytm import TM, Boundary, Actor, Datastore, Process, Dataflow

tm = TM("Customer Portal")
tm.description = "A customer self-service portal for contracts, price lists and catalog data. The customer registration is out of scope since that is managed through another workflow"

tzInternet = Boundary("Internet")
tzDMZ = Boundary("DMZ")
tzPrivateNetwork = Boundary("Internal Network")

storeCustomerFiles = Datastore("Customer File Storage")
storeCustomerFiles.inBoundary = tzPrivateNetwork

storeDatabase = Datastore("User and Customer Database")
storeDatabase.inBoundary = tzPrivateNetwork

storeTaskMessageQueue = Datastore("Task Message Queue")
storeTaskMessageQueue.inBoundary = tzPrivateNetwork

# Actors

actorSalesDepartment = Actor("Sales Department")
actorSalesDepartment.inBoundary = tzPrivateNetwork

actorCustomer = Actor("Customer")
actorCustomer.inBoundary = tzInternet

actorUnregisteredCustomer = Actor("Unregistered Customer")
actorUnregisteredCustomer.inBoundary = tzInternet

# Processes

procAuthenticationService = Process("User Authentication Service")
procAuthenticationService.inBoundary = tzPrivateNetwork

procWebsiteSalesDepartment = Process("Website for Sales Department")
procWebsiteSalesDepartment.inBoundary = tzPrivateNetwork

procWebsiteCustomers = Process("Website for Customers")
procWebsiteCustomers.inBoundary = tzDMZ

procPriceListGenerationService = Process("PriceList Generation Service")
procPriceListGenerationService.inBoundary = tzPrivateNetwork

# Dataflows

dfRegisterCustomer = Dataflow(actorUnregisteredCustomer, procWebsiteCustomers, "Register as customer")
dfLoginCustomer = Dataflow(actorUnregisteredCustomer, procWebsiteCustomers, "Login customer")

dfCustomerAccessRequestPriceList = Dataflow(actorCustomer, procWebsiteCustomers, "Request price list generation")
dfCustomerAccessPriceListGenStatus = Dataflow(procWebsiteCustomers, actorCustomer, "Access generated price lists")
dfCustomerAccessGenericFile = Dataflow(procWebsiteCustomers, actorCustomer, "Access generic files (Contracts, etc.)")

dfGetCustomerFile = Dataflow(storeCustomerFiles, procWebsiteCustomers, "Get file for customer download")

dfRequestFileGeneration = Dataflow(procWebsiteCustomers, storeCustomerFiles, "Request price list generation")



dfAuthenticateCustomer = Dataflow(procWebsiteCustomers, procAuthenticationService, "Authenticate customer")


dfCreateCustomerAccountRequest = Dataflow(procAuthenticationService, storeDatabase, "Create customer account request")

dfCreateCustomerAccount = Dataflow(procAuthenticationService, storeDatabase, "Create customer account")

dfGetCustomerCredentials = Dataflow(procAuthenticationService, storeDatabase, "Get customer credentials")

dfDeleteCustomer = Dataflow(procAuthenticationService, storeDatabase, "Delete customer account")

dfSalesLogin = Dataflow(actorSalesDepartment, procWebsiteSalesDepartment, "Login")

dfSalesApproveCustomer = Dataflow(actorSalesDepartment, procWebsiteSalesDepartment, "Approve customer account request")

dfSalesDeleteCustomer = Dataflow(actorSalesDepartment, procWebsiteSalesDepartment, "Delete customer account")

dfSalesUploadCustomerContract = Dataflow(actorSalesDepartment, procWebsiteSalesDepartment, "Upload customer contract file")

dfSalesAuthenticate = Dataflow(procWebsiteSalesDepartment, procAuthenticationService, "Authenticate sales department")

dfUserApproveCustomer = Dataflow(procWebsiteSalesDepartment, procAuthenticationService, "Approve customer account request")

dfUserDeleteCustomer = Dataflow(procWebsiteSalesDepartment, procAuthenticationService, "Delete customer account")

dfStoreCustomerContractFile = Dataflow(procWebsiteSalesDepartment, storeCustomerFiles, "Store customer contract file")



dfCustomerCheckTaskStatus = Dataflow(storeTaskMessageQueue, procWebsiteCustomers, "Inform about customer's task status")
dfCustomerCheckTaskStatus.protocol = "TCP"
dfCustomerCheckTaskStatus.dstPort = 1234


dfStartPriceListGenerationTask = Dataflow(storeTaskMessageQueue, procPriceListGenerationService, "Request new price list generation")
dfStartPriceListGenerationTask.protocol = "TCP"
dfStartPriceListGenerationTask.dstPort = 1234

dfStatusPriceListGenerationTask = Dataflow(procPriceListGenerationService, storeTaskMessageQueue, "Inform about task status")
dfStatusPriceListGenerationTask.protocol = "TCP"
dfStatusPriceListGenerationTask.dstPort = 1234

dfPriceListFileSave = Dataflow(procPriceListGenerationService, storeCustomerFiles, "Store generated price list file")
dfPriceListFileSave.protocol = "S3"
dfPriceListFileSave.dstPort = 443


tm.process()