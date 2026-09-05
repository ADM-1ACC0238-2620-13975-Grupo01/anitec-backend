# Learning Center Platform — REST API Technical Stories

## Overview

This document contains API-focused technical stories intended for frontend or mobile developers integrating with the Learning Center Platform REST API (ASP.NET Core). 

Common conventions
- Base path: `/api/v1`
- Response codes reflect controller behavior in this repository (e.g., list endpoints may return `200 OK` with an empty array or `404 Not Found` depending on controller; see each story).

---

## Bounded Context: Publishing

The Publishing context manages tutorials and categories used across the platform.

### TS-PUB-001 — Create a Tutorial
As a frontend developer, I want to create a tutorial through the API so that I can implement the tutorial creation flow in the instructor UI, persist tutorial metadata, and receive the created tutorial to display or link from course/learning-path screens.

Acceptance criteria:
- Scenario: Successful create
  - Given a POST request to `/api/v1/tutorials` is received with a body containing the attributes: Title, Summary, CategoryId
  - When the API validates and persists the tutorial
  - Then the API responds `201 Created` and returns the created tutorial with attributes exposed by `TutorialResource` (id, title, summary, categoryId, etc.)

- Scenario: Validation error
  - Given a POST request to `/api/v1/tutorials` is received with missing or invalid attributes (e.g., Title is empty or CategoryId is missing)
  - When the API rejects the request due to validation
  - Then the API responds `400 Bad Request` and returns an error payload describing validation issues

---

### TS-PUB-002 — Get a Tutorial by id
As a frontend developer, I want to fetch a tutorial by `{tutorialId}` so that I can implement the tutorial detail view, show metadata and any attached assets (videos), and handle not-found states in the UI.

Acceptance criteria:
- Scenario: Found
  - Given a GET request to `/api/v1/tutorials/{tutorialId}` is received
  - When the API finds the tutorial
  - Then the API responds `200 OK` and returns `TutorialResource`

- Scenario: Not found
  - Given a GET request to `/api/v1/tutorials/{tutorialId}` is received for a non-existent `{tutorialId}`
  - When the API does not find the tutorial
  - Then the API responds `404 Not Found` and returns an error payload

---

### TS-PUB-003 — Get all Tutorials
As a frontend developer, I want to list tutorials so that I can implement tutorial catalogue/listing screens and support category filtering via the category endpoints.

Acceptance criteria:
- Given a GET request to `/api/v1/tutorials` is received
- When the API returns tutorials
- Then the API responds `200 OK` with an array of `TutorialResource` items

---

### TS-PUB-004 — Add a Video Asset to a Tutorial
As a frontend developer, I want to add a video asset to a tutorial so that I can implement an instructor workflow to attach media to tutorials and obtain the updated tutorial resource for the UI.

Acceptance criteria:
- Scenario: Successful add
  - Given a POST request to `/api/v1/tutorials/{tutorialId}/videos` is received with a body containing the attribute: VideoUrl
  - When the API attaches the video to the tutorial
  - Then the API responds `201 Created` and returns the updated `TutorialResource`

- Scenario: Bad request
  - Given a POST request to `/api/v1/tutorials/{tutorialId}/videos` is received with an invalid payload (e.g., VideoUrl is missing or not a valid URL)
  - When the API cannot process the request
  - Then the API responds `400 Bad Request`

---

### TS-PUB-005 — Categories: Create, Get by id, Get all, and Tutorials by Category
As a frontend developer, I want to manage categories and obtain tutorials by category so that I can implement category-based browsing and tutorial organization in the UI.

Acceptance criteria:
- Create category
  - Given a POST request to `/api/v1/categories` is received with a body containing the attribute: Name
  - When the API validates and persists the category
  - Then the API responds `201 Created` and returns `CategoryResource`
  - On failure the API responds `400 Bad Request`

- Get category by id
  - Given a GET request to `/api/v1/categories/{categoryId}` is received
  - When the API finds the category
  - Then the API responds `200 OK` and returns `CategoryResource`
  - If not found, `404 Not Found`

- Get all categories
  - Given a GET request to `/api/v1/categories` is received
  - Then the API responds `200 OK` with an array of `CategoryResource` items

- Get all tutorials by category
  - Given a GET request to `/api/v1/categories/{categoryId}/tutorials` is received
  - When the API finds tutorials for the category
  - Then the API responds `200 OK` with an array of `TutorialResource` items

---

## Bounded Context: Profiles

The Profiles context manages learner/instructor profiles.

### TS-PROF-001 — Create a Profile
As a frontend developer, I want to create a profile through the API so that I can implement profile creation and onboarding flows in the UI, store contact/address data, and receive the created profile to continue onboarding or navigation.

Acceptance criteria:
- Scenario: Successful create
  - Given a POST request to `/api/v1/profiles` is received with a body containing the attributes: FirstName, LastName, Email, Street, Number, City, PostalCode, Country
  - When the API validates and persists the profile
  - Then the API responds `201 Created` and returns `ProfileResource` (id, fullName, email, streetAddress)

- Scenario: Validation error
  - Given a POST request to `/api/v1/profiles` is received with missing or invalid attributes (e.g., blank FirstName or invalid Email)
  - When the API rejects the request due to validation
  - Then the API responds `400 Bad Request` and returns an error payload

---

### TS-PROF-002 — Get Profile by id
As a frontend developer, I want to fetch a profile by `{profileId}` so that I can implement profile detail screens, pre-fill forms when editing, and handle not-found states.

Acceptance criteria:
- Given a GET request to `/api/v1/profiles/{profileId}` is received
- When found, respond `200 OK` with `ProfileResource`
- When not found, respond `404 Not Found`

---

### TS-PROF-003 — Get all Profiles
As a frontend developer, I want to list profiles so that I can implement admin or directory views and handle empty results appropriately.

Acceptance criteria:
- Given a GET request to `/api/v1/profiles` is received
- Then the API responds `200 OK` with an array of `ProfileResource` items

---

## Bounded Context: IAM (Authentication & Users)

The IAM context exposes authentication endpoints and user commands used by the platform.

### TS-IAM-001 — Sign-in
As a frontend developer, I want to sign in users through the API so that I can implement authentication flows, obtain a JWT token, and populate the client session with the authenticated user's data.

Acceptance criteria:
- Given a POST request to `/api/v1/authentication/sign-in` is received with a body containing the attributes: Username, Password
- When credentials are valid
- Then the API responds `200 OK` and returns `AuthenticatedUserResource` (user info and token)

- When credentials are invalid
- Then the API responds with an error (controller returns `404` or `400` depending on implementation)

---

### TS-IAM-002 — Sign-up
As a frontend developer, I want to sign up a new user through the API so that I can implement registration flows, capture initial user credentials, and receive confirmation to continue onboarding.

Acceptance criteria:
- Given a POST request to `/api/v1/authentication/sign-up` is received with a body containing the attributes: Username, Password
- When the API creates the user
- Then the API responds `200 OK` and returns a confirmation message (per current controller behavior)

- On validation or create failure, the API responds `400 Bad Request` (or the controller-specific error response)

---

## Notes & Next steps
- The stories above are scoped to the Publishing, Profiles, and IAM bounded contexts available in this repository and are aligned with the actual controller endpoints and response behavior.
- I can now generate example request/response bodies for each story, or produce an OpenAPI fragment that matches the existing controllers. Which would you prefer?

---

