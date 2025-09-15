/**
 * Test utility for verifying auth functionality with new API structure
 * This is a temporary file for testing the new login structure
 */

export const mockLoginResponse = {
  "isSuccess": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenExpiration": "2025-06-30T22:31:43.445164Z",
    "user": {
      "id": "57db5bd5-29f1-42f1-ab9d-4dba8a36c1af",
      "email": "jorge@hotel.fake",
      "userName": "jorge",
      "firstName": "jorge",
      "lastName": "",
      "phoneNumber": null,
      "institucionId": 1,
      "institucionName": "Hotel Test",
      "createdAt": "2025-06-29T10:21:35.617373",
      "lastLoginAt": "2025-06-29T22:31:43.311175Z",
      "isActive": true,
      "forcePasswordChange": false,
      "roles": ["User"]
    }
  },
  "errors": [],
  "message": "Login successful"
};

export const mockErrorResponse = {
  "isSuccess": false,
  "data": null,
  "errors": [
    "The Email field is required.",
    "The Email field is not a valid e-mail address."
  ],
  "message": "Validation failed"
};

/**
 * Test function to verify role checking logic
 */
export function testRoleChecking() {
  const testCases = [
    {
      name: "User with User role should access User routes",
      userRoles: ["User"],
      allowedRoles: ["User", "Admin"],
      expected: true
    },
    {
      name: "User with Admin role should access Admin routes",
      userRoles: ["Admin"],
      allowedRoles: ["Admin"],
      expected: true
    },
    {
      name: "User without required role should be denied",
      userRoles: ["User"],
      allowedRoles: ["Admin"],
      expected: false
    },
    {
      name: "Legacy numeric role should work",
      userRoles: [1],
      allowedRoles: [1, 2],
      expected: true
    }
  ];

  const hasRole = (userRoles, allowedRoles) => {
    if (!userRoles || !allowedRoles) return false;
    const rolesArray = Array.isArray(userRoles) ? userRoles : [userRoles];
    return rolesArray.some(role => allowedRoles.includes(role));
  };

  console.log("🧪 Testing role checking logic...");
  
  testCases.forEach(testCase => {
    const result = hasRole(testCase.userRoles, testCase.allowedRoles);
    const status = result === testCase.expected ? "✅ PASS" : "❌ FAIL";
    console.log(`${status} ${testCase.name}`);
  });
  
  console.log("🧪 Role checking tests completed");
}

/**
 * Validate new API response structure
 */
export function validateLoginResponse(response) {
  const requiredFields = ['isSuccess', 'data', 'errors', 'message'];
  const requiredDataFields = ['token', 'tokenExpiration', 'user'];
  const requiredUserFields = ['id', 'email', 'userName', 'institucionId', 'roles'];

  console.log("🔍 Validating login response structure...");

  // Check top-level fields
  const missingFields = requiredFields.filter(field => !(field in response));
  if (missingFields.length > 0) {
    console.error("❌ Missing top-level fields:", missingFields);
    return false;
  }

  if (response.isSuccess && response.data) {
    // Check data fields
    const missingDataFields = requiredDataFields.filter(field => !(field in response.data));
    if (missingDataFields.length > 0) {
      console.error("❌ Missing data fields:", missingDataFields);
      return false;
    }

    // Check user fields
    const missingUserFields = requiredUserFields.filter(field => !(field in response.data.user));
    if (missingUserFields.length > 0) {
      console.error("❌ Missing user fields:", missingUserFields);
      return false;
    }

    // Check roles is array
    if (!Array.isArray(response.data.user.roles)) {
      console.error("❌ User roles should be an array");
      return false;
    }
  }

  console.log("✅ Login response structure is valid");
  return true;
}

// Run tests if this file is imported
if (typeof window !== 'undefined') {
  console.log("🚀 Auth tests loaded. Run testRoleChecking() and validateLoginResponse() to test.");
}