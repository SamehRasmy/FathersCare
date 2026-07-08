# Rooms Module

Rooms manages the care home physical accommodation model:

- Floors with unique operational numbers.
- Rooms with capacity and availability.
- Resident-to-room assignment with occupancy history.
- Room capacity editing with validation against current occupancy.
- Soft deleting empty rooms only.
- A live room board grouped by floor.
- Maintenance requests linked to rooms and optionally to residents.
- Soft operational completion for maintenance requests with audit logging.

The Web project must call `IRoomManagementService` only. Room occupancy rules, capacity validation, soft delete checks, and maintenance state changes live in the Application/Infrastructure implementation and are not handled directly inside Razor components.
