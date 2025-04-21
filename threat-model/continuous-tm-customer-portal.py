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

procWebsiteSalesDepartment = Process("Website for Sales Department")
procWebsiteSalesDepartment.inBoundary = tzPrivateNetwork

procWebsiteCustomers = Process("Website for Customers")
procWebsiteCustomers.inBoundary = tzDMZ

procPriceListGenerationService = Process("PriceList Generation Service")
procPriceListGenerationService.inBoundary = tzPrivateNetwork

# Dataflows

dfRegisterCustomer = Dataflow(actorUnregisteredCustomer, procWebsiteCustomers, "Register as customer")
dfLoginCustomer = Dataflow(actorUnregisteredCustomer, procWebsiteCustomers, "Login")

dfCustomerAccessRequestPriceList = Dataflow(actorCustomer, procWebsiteCustomers, "Request price list generation")
dfCustomerAccessPriceListGenStatus = Dataflow(procWebsiteCustomers, actorCustomer, "Access generated price lists")
dfCustomerAccessGenericFile = Dataflow(procWebsiteCustomers, actorCustomer, "Access generic files (Contracts, etc.)")

Dataflow(storeCustomerFiles, procWebsiteCustomers, "Get customer generic file list")
Dataflow(storeCustomerFiles, procWebsiteCustomers, "Get file contents for customer download")

Dataflow(procWebsiteCustomers, storeTaskMessageQueue, "Create task for price list generation")



Dataflow(procWebsiteCustomers, procAuthenticationService, "Authenticate customer")
Dataflow(procWebsiteCustomers, procAuthenticationService, "Create new customer account")


Dataflow(procAuthenticationService, storeDatabase, "Create user account")

Dataflow(procAuthenticationService, storeDatabase, "Get user accounts")

Dataflow(procAuthenticationService, storeDatabase, "Update customer account")


Dataflow(actorUnregisteredInternalUser, procWebsiteSalesDepartment, "Register as admin or sales department user")

Dataflow(actorAdminOrSalesDepartment, procWebsiteSalesDepartment, "Login")

Dataflow(actorAdminOrSalesDepartment, procWebsiteSalesDepartment, "Approve / deactivate account")

Dataflow(actorAdminOrSalesDepartment, procWebsiteSalesDepartment, "Upload customer generic file")

Dataflow(procWebsiteSalesDepartment, procAuthenticationService, "Register new internal account")

Dataflow(procWebsiteSalesDepartment, procAuthenticationService, "Authenticate internal user")

Dataflow(procWebsiteSalesDepartment, procAuthenticationService, "Approve / deactivate account (internal or customer)")

Dataflow(procWebsiteSalesDepartment, storeCustomerFiles, "Store generic customer file (Contracts, etc.)")



Dataflow(storeTaskMessageQueue, procWebsiteCustomers, "Get customer's tasks")

Dataflow(storeTaskMessageQueue, procPriceListGenerationService, "Get next price list generation task")

Dataflow(procPriceListGenerationService, storeTaskMessageQueue, "Inform about task status")

Dataflow(procPriceListGenerationService, storeCustomerFiles, "Store generated price list file")


tm.process()