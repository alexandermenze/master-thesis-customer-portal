#!/usr/bin/env python3

from pytm import TM, Boundary, Actor, Datastore, Process, Dataflow

tm = TM("Customer Portal")
tm.description = "A customer self-service portal for contracts, price lists and catalog data. The customer registration is out of scope since that is managed through another workflow"

tzDMZ = Boundary("DMZ")
tzInternet = Boundary("Internet")
tzPrivateNetwork = Boundary("Internal Network")

# Stores
storeLogs = Datastore("Log Database")
storeLogs.inBoundary = tzPrivateNetwork

storeCustomerFiles = Datastore("Customer File Storage")
storeCustomerFiles.inBoundary = tzPrivateNetwork

storeDatabase = Datastore("User and Customer Database")
storeDatabase.inBoundary = tzPrivateNetwork

storeTaskMessageQueue = Datastore("Task Message Queue")
storeTaskMessageQueue.inBoundary = tzPrivateNetwork

# Actors
actorAdminOrSalesDepartment = Actor("Admin / Sales Department")
actorAdminOrSalesDepartment.inBoundary = tzPrivateNetwork

actorUnregisteredInternalUser = Actor("Unregistered Internal User")
actorUnregisteredInternalUser.inBoundary = tzPrivateNetwork

actorCustomer = Actor("Customer")
actorCustomer.inBoundary = tzInternet

actorUnregisteredCustomer = Actor("Unregistered Customer")
actorUnregisteredCustomer.inBoundary = tzInternet

# Processes
procAuthenticationService = Process("User Authentication Service")
procAuthenticationService.inBoundary = tzPrivateNetwork
procAuthenticationService.inboundCallPoints = [
    "CustomerPortal.UserAuthService.*"]

procWebsiteSalesDepartment = Process("Website for Sales Department")
procWebsiteSalesDepartment.inBoundary = tzPrivateNetwork
procWebsiteSalesDepartment.inboundCallPoints = [
    "CustomerPortal.InternalWebsite.*"]

procWebsiteCustomers = Process("Website for Customers")
procWebsiteCustomers.inBoundary = tzDMZ
procWebsiteCustomers.inboundCallPoints = [
    "CustomerPortal.CustomerWebsite.*"]

procPriceListGenerationService = Process("PriceList Generation Service")
procPriceListGenerationService.inBoundary = tzPrivateNetwork
procPriceListGenerationService.inboundCallPoints = [
    "CustomerPortal.PriceListGenerationService.*"]

# Dataflows

# - Logging

# Case 1.
# System process calls external process (or store)
# Marked by
#  - ...
# Is
#  - Outbound-Data-Push-Call-Point
# Maps to
#  - Dataflow-Exit-Point
internalWebsiteLogFileUpload = Dataflow(
    procWebsiteSalesDepartment, storeLogs, "Log file upload")
internalWebsiteLogFileUpload.codeLocation = "CustomerPortal.InternalWebsite.Pages.UploadCustomerFile.OnPostAsync"
internalWebsiteLogFileUpload.callPoints = {
    "outbound": [
        "Microsoft.Extensions.Logging.LoggerExtensions.*"
    ]
}

Dataflow(procWebsiteSalesDepartment, storeLogs, "Log errors")

Dataflow(procAuthenticationService, storeLogs, "Log user auth events")
Dataflow(procAuthenticationService, storeLogs, "Log approval and deactivation")

Dataflow(procPriceListGenerationService, storeLogs,
         "Log price list generation performance")

Dataflow(procWebsiteCustomers, storeLogs,
         "Log price list generation triggered")
Dataflow(procWebsiteCustomers, storeLogs,
         "Log customer file download with user id")

# - Customer Interactions
Dataflow(actorUnregisteredCustomer,
         procWebsiteCustomers, "Register as customer")
Dataflow(actorUnregisteredCustomer, procWebsiteCustomers, "Login")

Dataflow(actorCustomer, procWebsiteCustomers, "Request price list generation")
Dataflow(procWebsiteCustomers, actorCustomer, "Access generated price lists")
Dataflow(procWebsiteCustomers, actorCustomer,
         "Access generic files (Contracts, etc.)")

# - Customer File Storage
Dataflow(storeCustomerFiles, procWebsiteCustomers,
         "Get customer generic file list")
Dataflow(storeCustomerFiles, procWebsiteCustomers,
         "Get file contents for customer download")

Dataflow(procWebsiteSalesDepartment, storeCustomerFiles,
         "Store generic customer file (Contracts, etc.)")

Dataflow(procPriceListGenerationService, storeCustomerFiles,
         "Store generated price list file")

# - Price List Generation
Dataflow(storeTaskMessageQueue, procWebsiteCustomers, "Get customer's tasks")
Dataflow(storeTaskMessageQueue, procPriceListGenerationService,
         "Get next price list generation task")

Dataflow(procWebsiteCustomers, storeTaskMessageQueue,
         "Create task for price list generation")
Dataflow(procPriceListGenerationService,
         storeTaskMessageQueue, "Inform about task status")

# - User Authentication
Dataflow(procAuthenticationService, storeDatabase, "Create user account")
Dataflow(procAuthenticationService, storeDatabase, "Get user accounts")
Dataflow(procAuthenticationService, storeDatabase, "Update customer account")

Dataflow(procWebsiteCustomers, procAuthenticationService,
         "Authenticate customer")
Dataflow(procWebsiteCustomers, procAuthenticationService,
         "Create new customer account")


registerInternalAccount = Dataflow(procWebsiteSalesDepartment, procAuthenticationService,
                                   "Register new internal account")
registerInternalAccount.outboundCallPoints = [
    {
        "dataSource": "CustomerPortal.InternalWebsite.Pages.RegisterModel.OnPostAsync",
        "dataSink": "System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync<InputModel>"
    }
]


Dataflow(procWebsiteSalesDepartment, procAuthenticationService,
         "Authenticate internal user")
Dataflow(procWebsiteSalesDepartment, procAuthenticationService,
         "Approve / deactivate account (internal or customer)")

# - Internal User Interaction
Dataflow(actorUnregisteredInternalUser, procWebsiteSalesDepartment,
         "Register as admin or sales department user")

Dataflow(actorAdminOrSalesDepartment, procWebsiteSalesDepartment, "Login")

Dataflow(actorAdminOrSalesDepartment, procWebsiteSalesDepartment,
         "Approve / deactivate account")

uploadCustomerFile = Dataflow(actorAdminOrSalesDepartment, procWebsiteSalesDepartment,
                              "Upload customer generic file")
# Case 4.
# Outside process or entity calls into system
# Marked by
#  - No Outbound-Call-Points
# Is
#  - Inbound-Framework-Call-Point
# Maps to
#  - Dataflow-Entry-Point
uploadCustomerFile.callPoints = {
    "inbound": [
        "CustomerPortal.InternalWebsite.Pages.UploadCustomerFile.OnPostAsync"
    ]
}


tm.process()
