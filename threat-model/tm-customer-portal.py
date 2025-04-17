#!/usr/bin/env python3

from pytm import TM, Boundary, Actor, Datastore, Process, Dataflow

tm = TM("Customer Portal")
tm.description = "A customer self-service portal for contracts, pricelists and catalog data. The customer registration is out of scope since that is managed through another workflow"

tzInternet = Boundary("Internet")
tzDMZ = Boundary("DMZ")
tzPrivateNetwork = Boundary("Internal Network")

#dataGeneratedFiles = Data("Generated Catalogs and PriceLists", )
#dataCustomerDatabase = Data("Database for Authentication and Master Data")
#dataTaskMessageQueue = Data("Task Message Queue")

storeCustomerFiles = Datastore("Customer File Storage")
storeCustomerFiles.inBoundary = tzPrivateNetwork

storeDatabase = Datastore("User and Customer Database")
storeDatabase.inBoundary = tzPrivateNetwork

actorSalesDepartment = Actor("Sales Department")
actorSalesDepartment.inBoundary = tzPrivateNetwork

actorCustomer = Actor("Customer")
actorCustomer.inBoundary = tzInternet

actorUnregisteredCustomer = Actor("Unregistered Customer")
actorUnregisteredCustomer.inBoundary = tzInternet

procAuthenticationService = Process("User Authentication Service")
procAuthenticationService.inBoundary = tzPrivateNetwork

procWebsiteSalesDepartment = Process("Website for Sales Department")
procWebsiteSalesDepartment.inBoundary = tzPrivateNetwork

procWebsiteCustomers = Process("Website for Customers")
procWebsiteCustomers.inBoundary = tzDMZ

# Dataflows

dfRegisterCustomer = Dataflow(actorUnregisteredCustomer, procWebsiteCustomers, "Register new customer")
dfRegisterCustomer.protocol = "HTTPS"
dfRegisterCustomer.dstPort = 443

dfCustomerAccessWebsite = Dataflow(actorCustomer, procWebsiteCustomers, "Access task status; Request file generation")
dfCustomerAccessWebsite.protocol = "HTTPS"
dfCustomerAccessWebsite.dstPort = 443

dfCustomerDownloadsFile = Dataflow(procWebsiteCustomers, actorCustomer, "Download file")
dfCustomerDownloadsFile.protocol = "HTTPS"
dfCustomerDownloadsFile.dstPort = 443

dfRetrieveCustomerFile = Dataflow(storeCustomerFiles, procWebsiteCustomers, "Retrieve file for customer download")
dfRetrieveCustomerFile.protocol = "S3"
dfRetrieveCustomerFile.dstPort = 443

dfCheckCredentials = Dataflow(procWebsiteCustomers, procAuthenticationService, "Check credentials")
dfCheckCredentials.protocol = "HTTPS"
dfCheckCredentials.dstPort = 443

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


def fileGenerationProcesses(customerFileStorage: Datastore, taskStatusProcess: Process):
    storeTaskMessageQueue = Datastore("Task Message Queue")
    storeTaskMessageQueue.inBoundary = tzPrivateNetwork

    procCatalogGenerationService = Process("Catalog Generation Service")
    procCatalogGenerationService.inBoundary = tzPrivateNetwork

    procPriceListGenerationService = Process("PriceList Generation Service")
    procPriceListGenerationService.inBoundary = tzPrivateNetwork

    dfCustomerCheckTaskStatus = Dataflow(storeTaskMessageQueue, taskStatusProcess, "Inform about customer's task status")
    dfCustomerCheckTaskStatus.protocol = "TCP"
    dfCustomerCheckTaskStatus.dstPort = 1234

    dfRequestFileGeneration = Dataflow(taskStatusProcess, storeTaskMessageQueue, "Request customer file generation")

    dfStartCatalogGenerationTask = Dataflow(storeTaskMessageQueue, procCatalogGenerationService, "Request new catalog generation")
    dfStartCatalogGenerationTask.protocol = "TCP"
    dfStartCatalogGenerationTask.dstPort = 1234

    dfEndCatalogGenerationTask = Dataflow(procCatalogGenerationService, storeTaskMessageQueue, "Inform about finished task")
    dfEndCatalogGenerationTask.protocol = "TCP"
    dfEndCatalogGenerationTask.dstPort = 1234

    dfCatalogGenerationFileSave = Dataflow(procCatalogGenerationService, customerFileStorage, "Store generated catalog file")
    dfCatalogGenerationFileSave.protocol = "S3"
    dfCatalogGenerationFileSave.dstPort = 443

    dfStartPriceListGenerationTask = Dataflow(storeTaskMessageQueue, procPriceListGenerationService, "Request new pricelist generation")
    dfStartPriceListGenerationTask.protocol = "TCP"
    dfStartPriceListGenerationTask.dstPort = 1234

    dfEndPriceListGenerationTask = Dataflow(procPriceListGenerationService, storeTaskMessageQueue, "Inform about finished task")
    dfEndPriceListGenerationTask.protocol = "TCP"
    dfEndPriceListGenerationTask.dstPort = 1234

    dfPriceListFileSave = Dataflow(procPriceListGenerationService, storeCustomerFiles, "Store generated pricelist file")
    dfPriceListFileSave.protocol = "S3"
    dfPriceListFileSave.dstPort = 443

    return 


fileGenerationProcesses(storeCustomerFiles, procWebsiteCustomers)

tm.process()