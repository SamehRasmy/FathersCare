# Database Map

Core tables:

- Tenants
- Users
- Roles
- Permissions
- AuditLogs
- Residents
- ResidentContacts
- ResidentDocuments
- ResidentMedicalProfiles
- ResidentVitalSigns
- ResidentAllergies
- ResidentHealthConditions
- Floors
- Rooms
- RoomOccupancies
- Medicines
- ResidentMedicines
- MedicineSchedules
- DoseAdministrations
- MedicineShelves
- MedicineStockTransactions
- DietTypes
- Meals
- ResidentDietPlans
- MealPlans
- MealDistributions
- KitchenInventoryItems
- KitchenStockTransactions
- Revenues
- Expenses
- FinancialPeriods
- Notifications
- SystemEvents
- Files

Every main operational table must include tenant, audit, and soft delete fields through the shared domain base entities.
