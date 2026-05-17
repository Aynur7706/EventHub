# EventHub Demo Flow

Use this script for a 2-3 minute project presentation.

## 1. Public User Flow

1. Open the home page.
2. Go to `Events`.
3. Search or filter events.
4. Open an event detail page.
5. Save the event or reserve a ticket.
6. Open `My Tickets` to show the calendar.
7. Open a ticket detail page to show the QR-style ticket code.
8. Open `My Profile` to show profile info, tickets and saved events.

## 2. Organizer Flow

1. Login as `organizer@eventhub.local`.
2. Open `Organizer Dashboard`.
3. Show revenue, tickets sold, published/pending/rejected stats.
4. Create or edit an event.
5. Explain that organizer-created events go to `PendingReview`.

## 3. Admin Flow

1. Login as `admin@eventhub.local`.
2. Open `Admin Dashboard`.
3. Open `Pending Events`.
4. Approve or reject an event with an admin note.
5. Open `Tickets`.
6. Show CSV export and ticket check-in.
7. Open `Messages`.
8. Generate an AI reply draft, edit it and save the reply.
9. Open `Audit Logs` to show tracked platform actions.

## 4. Key Technical Talking Points

- ASP.NET Core MVC architecture
- Entity Framework Core with a real database
- ASP.NET Identity authentication and roles
- Admin, Organizer and User workflows
- Repository Pattern and Unit of Work
- DTOs, ViewModels and Services
- Ticket lifecycle and QR-style validation
- AI assistant abstraction with fallback template provider
- Responsive dashboard UI

## 5. CV Description

EventHub — Advanced Event Management Platform

Built a full-featured role-based event management platform using ASP.NET Core MVC, Entity Framework Core, SQLite/SQL Server-ready configuration and ASP.NET Identity. Implemented organizer approval, event review workflows, ticket reservation, QR-style check-in, user profiles, saved events, admin dashboards, support message management, AI-assisted reply drafting, audit logs and CSV export following clean MVC architecture principles.
