## 🔄 **Complete Client Onboarding & Verification Flow**

### **Step 1: Bank User Creates a Client (Company/Organization)**

```http
POST /api/bankuser/clients
{
  "clientName": "ABC Corporation",
  "registerationNumber": "COMP123456",
  "address": "Mumbai, India",
  "bankId": 1,
  "bankName": "HDFC Bank"
}
```

**Status:** Client created with `VerificationStatus = "Pending"`

### **Step 2: Bank User Uploads Client Documents**

```http
POST /api/bankuser/clients/123/documents
FormData:
- File: business_license.pdf
- DocType: "BusinessLicense"
```

**Documents typically needed:**

- Business License
- KYC Documents
- Tax Certificates
- Bank Statements
- Address Proof
- Identity Proof

### **Step 3: Bank User Moves Client to "In Review"**

```http
PUT /api/bankuser/clients/123/verify
{
  "verificationStatus": "InReview",
  "notes": "Documents uploaded, starting verification process"
}
```

### **Step 4: Bank User Reviews Documents & Verifies Client**

```http
PUT /api/bankuser/clients/123/verify
{
  "verificationStatus": "Verified",
  "notes": "All documents verified successfully. Client approved."
}
```

**Now the client is VERIFIED and can access the system**

### **Step 5: Bank User Creates Client Users for the Verified Client**

```http
POST /api/bankuser/clients/123/users
{
  "userName": "abc_admin",
  "fullName": "John Doe",
  "email": "john@abccorp.com",
  "password": "SecurePass123",
  "clientId": 123
}
```

**Response includes plain text password for the Bank User to share with the Client User**

### **Step 6: Client User Logs In and Manages Their Operations**

- Manage beneficiaries
- Process payments
- Manage employees
- Disburse salaries
- Generate reports

## 📋 **Verification Status Flowchart:**

```
Pending → InReview → Verified → [Suspended] → Verified
    ↓        ↓         ↓           ↓
 Rejected → InReview → Verified → Rejected
```

## 🎯 **What Each Status Means:**

### **Pending**

- Client created but no documents uploaded yet
- Cannot create Client Users
- Basic information only

### **InReview**

- Documents uploaded and under review
- Bank User is verifying the documents
- Still cannot create Client Users

### **Verified**

- Client successfully verified
- Can create Client Users
- Full system access granted

### **Rejected**

- Verification failed (missing/invalid documents)
- Cannot create Client Users
- Can be moved back to InReview after corrections

### **Suspended**

- Previously verified client temporarily suspended
- Client Users cannot access system
- Can be re-verified or rejected

## 🔧 **Bank User Dashboard Operations:**

### **1. View Clients by Status**

```http
GET /api/bankuser/clients/verification-status/Pending
GET /api/bankuser/clients/verification-status/InReview
GET /api/bankuser/clients/pending-verification  (Pending + InReview)
```

### **2. Monitor Client Documents**

```http
GET /api/bankuser/clients/123/documents
```

### **3. Track Client Users**

```http
GET /api/bankuser/clients/123/users
```

## ⚡ **Real-World Workflow Example:**

**Monday 9:00 AM** - Bank User creates "Tech Solutions Ltd." → Status: **Pending**

**Monday 9:30 AM** - Uploads business license and KYC docs → Status: **Pending**

**Monday 10:00 AM** - Moves to review → Status: **InReview**

**Monday 2:00 PM** - Reviews documents, finds everything valid → Status: **Verified**

**Monday 2:30 PM** - Creates Client user for Tech Solutions Ltd. → Client User account created

**Monday 3:00 PM** - Client User logs in and starts managing their payroll

## 🛡️ **Security & Compliance:**

1. **No self-verification** - Clients cannot verify themselves
2. **Document trail** - All documents stored securely
3. **Audit log** - Every status change tracked
4. **Role separation** - Bank Users verify, Client Users operate
5. **Compliance** - Meets KYC/AML requirements

## ✅ **Benefits of This Flow:**

- **Structured process** - Clear steps from creation to verification
- **Security** - Prevents unauthorized access
- **Compliance** - Proper documentation and verification
- **Efficiency** - Bank Users can batch process verifications
- **Transparency** - Clear status tracking

**This flow ensures only legitimate, verified clients can access the banking system while maintaining proper regulatory compliance!** 🏦✅
