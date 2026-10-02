# CareNotes.API

REST API for a secure note-taking platform. Built with ASP.NET Web API (.NET 10) using clean architecture, MongoDB, JWT auth and role-based access control (User / Admin).

Frontend: [CareNotes.UI](https://github.com/AbdullahAlRana/CareNotes.UI)

## Structure

```
CareNotes.Server.slnx
src/
  CareNotes.Api              Controllers, JWT auth, exception handling
  CareNotes.Application      Services, DTOs, repository/security contracts
  CareNotes.Domain           Entities (User, Note, Post, Roles)
  CareNotes.Infrastructure   MongoDB schemas + indexes, repositories, BCrypt, JWT
```

## Run

Requires the .NET 10 SDK and MongoDB on `mongodb://localhost:27017` (change it under `Mongo` in `appsettings.json`).

```bash
dotnet run --project src/CareNotes.Api --launch-profile http
```

API base URL: `http://localhost:5177/api`

- Indexes are created on startup.
- In Development, an admin is seeded from `appsettings.Development.json`: `admin@carenotes.local` / `Admin@12345`.
- In Development, a random JWT signing key is generated at startup if `Jwt:Key` is empty, so tokens are invalidated on restart. Outside Development, `Jwt:Key` must be set (for example via an environment variable `Jwt__Key`).

## Deploy (Docker)

The `Dockerfile` at the repo root builds a production image. The app listens on `$PORT` when set (as Render does), otherwise on 8080. `GET /health` is a lightweight check for uptime monitors.

Required environment variables:

| Variable | Example |
|---|---|
| `Mongo__ConnectionString` | `mongodb+srv://user:pass@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority` |
| `Mongo__Database` | `carenotes` |
| `Jwt__Key` | 48+ random characters |
| `Cors__Origins__0` | `https://your-frontend.vercel.app` |
| `Seed__AdminEmail` / `Seed__AdminPassword` | Optional: seeds the first admin on startup |

## Roles

- **User**: create, update, delete and list their own notes; write posts; manage their own profile and interests.
- **Admin**: everything a User can do, plus manage users (add, remove, update, list) and view everyone's notes.

## API

| Method | Route | Access |
|---|---|---|
| POST | `/api/auth/register`, `/api/auth/login` | Public |
| GET/PUT | `/api/users/me` | User |
| GET | `/api/notes?page=&pageSize=` | User (own notes) |
| POST | `/api/notes` | User |
| GET/PUT/DELETE | `/api/notes/{id}` | Owner (GET also Admin) |
| GET | `/api/notes/all?ownerId=&page=` | Admin |
| GET/POST | `/api/users` | Admin |
| GET/PUT/DELETE | `/api/users/{id}` | Admin |
| GET | `/api/users/by-interest?interests=chess&interests=reading&page=` | Admin (Aggregation 1) |
| GET | `/api/posts?page=` | Public (feed) |
| GET | `/api/users/{id}/posts?page=` | Public (Aggregation 2, `$lookup`) |
| POST | `/api/posts` | User |
| DELETE | `/api/posts/{id}` | Author or Admin |

Every list endpoint is paginated (`page`, `pageSize` ≤ 100) and returns `{ items, page, pageSize, total, totalPages }`.

## Mongoose note

Mongoose is a Node.js library and can't be used from ASP.NET, so the API uses the official `MongoDB.Driver`. To keep the `schema.index(...)` convention visible for review, each collection has a schema class in `src/CareNotes.Infrastructure/Persistence/Schemas/` that declares its field mapping and indexes with `Index(...)`, the equivalent of Mongoose's `schema.index()`.

## Indexes

Only four indexes are declared. Everything else uses the built-in `_id` index. Lists sort by `_id` descending (ObjectIds are time-ordered), so "newest first" needs no extra `createdAt` index.

| Collection | Index | Supports |
|---|---|---|
| users | `{ email: 1 }` unique | Login, duplicate-email check, uniqueness |
| users | `{ interests: 1 }` (multikey) | `$match` stage of the group-by-interests aggregation |
| notes | `{ ownerId: 1, _id: -1 }` | "My notes" filter + sort + count; admin filter by owner; cascade delete |
| posts | `{ authorId: 1, _id: -1 }` | `$lookup` foreignField + sort inside the lookup; cascade delete |

Served by `_id_` alone: get user/note/post by id, the admin user list, the admin all-notes list, the post feed, the `$match` on the user in Aggregation 2, and the feed's author `$lookup`. Every query was checked with `explain()`. Each one runs as an `IXSCAN` / `EXPRESS_IXSCAN`, with no `COLLSCAN`.

## Aggregations

**1. Group by interests** (`UserRepository.GroupByInterestsAsync`) is exactly one `users.aggregate()` call:
`$match` (`interests: {$type: "string"}`, or `$in` when filtered, both IXSCAN on `interests`) → `$project` → `$unwind` → (`$match` again when filtered) → `$group` by interest → `$sort` → `$facet` { page items, total }.

**2. User posts** (`PostRepository.GetUserPostsAsync`) is a single pipeline on `users`:
`$match {_id}` → `$lookup` into `posts` (`localField: _id`, `foreignField: authorId`, sub-pipeline `$sort {_id:-1}` → `$facet` { page items, total }) → `$project`. The lookup uses the `authorId_id` index.

## Security

- Passwords are hashed with BCrypt (work factor 12).
- JWTs are HS256 and carry `sub`, `email`, `name` and `role` claims.
- Admin endpoints require the `Admin` role policy. Note and post ownership is enforced in the application services.
