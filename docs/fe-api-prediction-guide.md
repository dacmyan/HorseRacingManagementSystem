# Frontend API Integration Guide for Predictions

This guide explains how to integrate and use the Prediction (Dự đoán) endpoints in the frontend application.

## 1. Endpoints Overview

All prediction endpoints are nested under the Spectator controller path. Users must be authenticated with the `Spectator` role to access these APIs.

---

### A. Place a Prediction
Allows a Spectator to predict the winner of a specific race by choosing a race entry.

- **URL:** `/api/Spectator/predictions`
- **Method:** `POST`
- **Auth Required:** `Bearer <token>` (Role: `Spectator`)
- **Request Headers:**
  - `Content-Type: application/json`
  - `Authorization: Bearer <your-jwt-token>`

- **Request Body:**
```json
{
  "raceId": 1,
  "raceEntryId": 10
}
```

- **Response Body (Success - 200 OK):**
```json
{
  "message": "Prediction submitted successfully",
  "result": {
    "predictionId": 12,
    "raceId": 1,
    "raceEntryId": 10,
    "status": "Pending",
    "isCorrect": null,
    "point": 0,
    "predictedAt": "2026-06-14T09:41:00Z"
  }
}
```

---

### B. Get My Predictions
Allows a Spectator to retrieve all of their submitted predictions.

- **URL:** `/api/Spectator/predictions/my-predictions`
- **Method:** `GET`
- **Auth Required:** `Bearer <token>` (Role: `Spectator`)

- **Response Body (Success - 200 OK):**
```json
{
  "message": "Your predictions retrieved successfully",
  "result": [
    {
      "predictionId": 12,
      "raceId": 1,
      "raceEntryId": 10,
      "status": "Evaluated",
      "isCorrect": true,
      "point": 1,
      "predictedAt": "2026-06-14T09:41:00Z"
    }
  ]
}
```

---

### C. Get Prediction for a Specific Race
Retrieves the logged-in Spectator's prediction for a specific race.

- **URL:** `/api/Spectator/predictions/race/{raceId}`
- **Method:** `GET`
- **Auth Required:** `Bearer <token>` (Role: `Spectator`)

- **Response Body (Success - 200 OK):**
```json
{
  "message": "Prediction retrieved successfully",
  "result": {
    "predictionId": 12,
    "raceId": 1,
    "raceEntryId": 10,
    "status": "Pending",
    "isCorrect": null,
    "point": 0,
    "predictedAt": "2026-06-14T09:41:00Z"
  }
}
```

- **Response Body (Not Found - 404 Not Found):**
  Returned if the user has not placed any prediction for this race.
```json
{
  "message": "No prediction found for race ID 1."
}
```

---

## 2. Business Rules & Logic

- **Role Constraints:** Only users logged in as `Spectator` can submit predictions. Admins, Horse Owners, and Jockeys are unauthorized.
- **Race Status:** Predictions can ONLY be submitted when the race status is `"Scheduled"`. Once a race is started, finished, or cancelled, predictions are blocked.
- **One Prediction Limit:** A spectator can place exactly **one prediction** per race. Submitting a second prediction for the same race will result in a `400 BadRequest`.
- **Wallet Compatibility:** Placing a prediction is **free** and does not deduct any balance from the user's wallet.
- **Auto-Evaluation:** Predictions are automatically evaluated when the Admin publishes the race result.
  - If the predicted `raceEntryId` matches the winning race entry, `isCorrect` becomes `true` and the spectator gets `point = 1`.
  - If incorrect, `isCorrect` becomes `false` and `point = 0`.
  - The prediction status transitions to `"Evaluated"`.

---

## 3. Important DataType Mapping Notice

Please pay attention to parameter types when sending requests:
- **`predictionId`** is returned as a standard **`int`** (e.g., `12`).
- **`raceId`** and **`raceEntryId`** in the database are stored as **`bigint`** (representing C# `long`). However, in JSON, standard integer parsing works seamlessly. For maximum safety in JavaScript, they can be handled as numeric IDs.
- Future structural migrations might map them to `int` if the team decides to standardize, but currently they are fully compatible with integers in JSON.

---

## 4. Error Cases

### A. Unauthorized User (Non-Spectator trying to predict)
- **Status Code:** `403 Forbidden` or `400 BadRequest` (with message: `"Only Spectator users are allowed to make predictions."`)

### B. Race Already Started or Finished
- **Status Code:** `400 BadRequest`
- **Response:**
```json
{
  "message": "Cannot make prediction. Race status is 'Finished'. Predictions are only allowed for 'Scheduled' races."
}
```

### C. Duplicate Prediction Submission
- **Status Code:** `400 BadRequest`
- **Response:**
```json
{
  "message": "You have already made a prediction for race ID 1."
}
```

### D. Invalid Race Entry Selection
- **Status Code:** `400 BadRequest`
- **Response:**
```json
{
  "message": "Race entry with ID 999 is not registered in this race."
}
```
