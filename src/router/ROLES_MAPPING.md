# Role Mapping Documentation

## New Role Structure

The system now uses string-based roles instead of numeric IDs for better clarity and maintainability.

### Role Mapping

| Legacy ID | Legacy Name | New Role Name | Description |
|-----------|-------------|---------------|-------------|
| 1 | Director | Director | Full system access, highest authority |
| 2 | Admin | Administrator | Administrative functions, user management |
| 3 | Cajero | Cajero | Cashier operations, transactions |
| 4 | Mucama | Mucama | Housekeeping, room management |
| 5 | Cajero Stock | Cajero Stock | Inventory management, stock operations |
| 6 | ExcelenciaAdmin | Administrator | System administration (unified with Admin) |
| 7 | Usuario | User | Basic user access |
| 8 | Administrador del Sistema | Administrator | System administration (unified with Admin) |

### Role Groups

#### BASIC_ACCESS
- **Roles**: User, Mucama
- **Access**: Basic system features, room status

#### CASHIER_ACCESS  
- **Roles**: Cajero, Cajero Stock
- **Access**: Transaction processing, inventory, financial operations

#### ADMIN_ACCESS
- **Roles**: Administrator, Director
- **Access**: User management, system configuration, all features

#### OPERATIONAL
- **Roles**: Cajero, Cajero Stock, Mucama
- **Access**: Day-to-day operational features

#### STAFF_ACCESS
- **Roles**: Director, Administrator, Cajero Stock, Cajero, Mucama
- **Access**: All staff features except basic user functions

#### MANAGEMENT
- **Roles**: Director, Administrator
- **Access**: Management and administrative functions

### Menu Access Control

#### Menu1 - Operational Staff
- **Who can see**: Mucama, Cajero, Cajero Stock
- **Features**: Room management, basic operations

#### Menu2 - Administrative
- **Who can see**: Administrator, Director
- **Features**: User management, system settings

#### Menu3 - Staff Functions
- **Who can see**: All staff (excluding basic users)
- **Features**: Reports, advanced features

#### Menu4 - Staff Functions
- **Who can see**: All staff (excluding basic users)  
- **Features**: Additional operational tools

### Route Access Examples

#### Administrative Routes
- **Room Creation**: Administrator, Director
- **User Management**: Administrator, Director
- **System Settings**: Administrator, Director

#### Operational Routes
- **Inventory**: Administrator, Director, Cajero, Cajero Stock
- **Orders**: Cajero, Cajero Stock
- **Financial Operations**: Administrator, Director, Cajero, Cajero Stock

#### Basic Routes
- **Room Viewing**: All staff
- **Dashboard**: All authenticated users

### Implementation Notes

1. **Backward Compatibility**: The system supports both legacy numeric roles and new string roles during transition
2. **Role Hierarchy**: Director > Administrator > Cajero Stock > Cajero > Mucama > User
3. **Access Control**: Routes check for role arrays, allowing flexible permission assignment
4. **Menu Visibility**: Menus are dynamically shown based on user roles

### Migration Strategy

1. **Phase 1**: Support both old and new role formats
2. **Phase 2**: Gradually migrate all role checks to new string format
3. **Phase 3**: Remove legacy numeric role support (future)

This documentation should be updated as role requirements evolve.