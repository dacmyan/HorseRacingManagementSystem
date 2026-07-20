# Project Repository Management Rules

- This workspace consists of two separate Git repositories:
  - **Backend**: `HorseRacingManagementSystem` (remote repo: `dacmyan/HorseRacingManagementSystem`).
  - **Frontend**: `Horse-Tournament-Management-Frontend` (located at `../Horse-Tournament-Management-Frontend` or via junction `frontend/`).
- **Rule**: All modifications must be managed, committed, and pushed separately in their respective directories:
  - Frontend changes must be run and pushed under the frontend repository context.
  - Backend changes must be run and pushed under the backend repository context.
  - Since Backend `main` is protected, Backend changes must be pushed to a feature branch and a PR link provided.
