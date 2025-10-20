# GearShare – Simple Technical Specs & User Requirements (v1.1)

**Date:** 2025-10-14  

---

## 🧭 1. Project Overview

**GearShare** is a peer-to-peer (P2P) rental application for equipment such as **sports**, **photo/video**, and **DIY** gear.

**Tech Stack:**
- **Backend:** ASP.NET Core Web API + Entity Framework Core (PostgreSQL) + Identity/JWT  
- **Frontend:** Angular  

---

## 👥 2. User Roles

- **Renter (logged-in user):** Can request rentals (bookings) and view their bookings.  
- **Owner (logged-in user):** Manages their **Items** and **Listings**, and approves/rejects booking requests.  
- **Admin:** Can moderate all content (future: users and reviews).  

---

## 💡 3. User Requirements

### 3.1 Browse Listings
- View active listings with title, cover image, price/day, deposit, and city. ✅  
- Browse page displays images via absolute URLs from API. ✅  

### 3.2 Authentication
- Register and log in; receive a **JWT token** for protected routes. ✅  
- System enforces role-based permissions (OWNER / RENTER / ADMIN). ✅  

### 3.3 Owner Features
- **Items CRUD:** Create, edit, delete owned items. ✅  
- **Images:** Upload/delete item images; deleting an item also removes files from disk. ✅  
- **Listings CRUD:** Create, edit, delete listings for owned items. ✅  
- View **PENDING** booking requests and **Accept/Reject** them. ✅  

### 3.4 Renter Features
- Choose date range on a listing and click **“Rent”** → message “waiting for approval.” ✅  
- “My Rentals” page shows all bookings and their statuses. ✅  

### 3.5 Admin Features
- Can delete any Item/Listing (override). ✅  
- Future: user and review moderation. ⏳ *(TO DO)*  

### 3.6 Extra (Planned)
- Search and filters (category, price, location). ⏳  
- Reviews CRUD after rental completion (rating + comment). ⏳  
- Admin dashboard (stats + moderation). ⏳  
- CSV export for basic reports. ⏳  

---

## ⚙️ 4. Rules & Validations

- **Booking intervals:** `[StartDate, EndDate)` with `EndDate > StartDate` (min 1 day). ✅  
- **No overlap** with existing **ACCEPTED** bookings for the same listing. ✅  
- Only the **listing owner** (or **admin**) can Accept/Reject. ✅  
- Only **owner/admin** can modify Items/Listings/Images. ✅  
- **CORS** enabled for `http://localhost:4200`; images served from `wwwroot/uploads`. ✅  

---

## 🧩 5. API Endpoints (Summary)

| Area | Methods & Routes | Status |
|------|------------------|--------|
| **Auth** | `POST /auth/register`, `POST /auth/login`, `GET /auth/me` | ✅ |
| **Items** | `GET /items`, `GET /items/{id}`, `POST/PUT/DELETE /items` | ✅ |
| **Item Images** | `POST /items/{id}/images`, `DELETE /items/{id}/images/{imageId}` | ✅ |
| **Listings** | `GET /listings`, `GET /listings/{id}`, `POST/PUT/DELETE /listings` | ✅ |
| **Bookings** | `POST /bookings` (create **PENDING**, check overlap, calc total) | ✅ |
| | `GET /bookings/mine` (renter) | ✅ |
| | `GET /bookings/owner/pending` (owner) | ✅ |
| | `PATCH /bookings/{id}/status` (Accept/Reject) | ✅ |
| | `GET /bookings/listing/{listingId}/availability` | ✅ |

---

## 🧰 6. Quick Setup (Dev)

### Backend
```bash
# 1. Set your ConnectionStrings in appsettings.json
# 2. Apply migrations
dotnet ef database update
# 3. Run the API
dotnet run
```

### Frontend
```bash
npm install
ng serve
```

Then configure `environment.apiUrl` to point to your backend API.

---

## 🚀 7. Remaining for Delivery (Week 4–5)

- Search & filters  
- Reviews CRUD  
- Admin moderation tools  
- UI polish (messages, pagination)  
- README with screenshots  

---

✅ = DONE  ⏳ = TO DO
