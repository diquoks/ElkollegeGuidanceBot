import pyquoks.managers.database

from .. import models


class DatabaseManager(pyquoks.managers.database.DatabaseManager):
    users: UsersDatabase


class UsersDatabase(pyquoks.managers.database.Database):
    _NAME = "users"

    _SQL = f"""\
CREATE TABLE IF NOT EXISTS {_NAME} (
id INTEGER PRIMARY KEY NOT NULL,
type INTEGER NOT NULL,
full_name TEXT NOT NULL,
phone_number TEXT,
email TEXT,
institution TEXT NOT NULL,
current_course TEXT,
possible_type TEXT NOT NULL,
timestamp INT NOT NULL
)"""

    def add_user(
            self,
            type_: models.UserType,
            full_name: str,
            phone_number: str | None,
            email: str | None,
            institution: str,
            current_course: str | None,
            possible_type: str,
            timestamp: int,
    ) -> None:
        cursor = self.cursor()

        cursor.execute(
            f"""\
INSERT OR IGNORE INTO {self._NAME} (
type,
full_name,
phone_number,
email,
institution,
current_course,
possible_type,
timestamp
)
VALUES (?, ?, ?, ?, ?, ?, ?, ?)""",
            (
                type_,
                full_name,
                phone_number,
                email,
                institution,
                current_course,
                possible_type,
                timestamp,
            ),
        )

        self.commit()

    def get_users_list(self) -> list[models.DatabaseUser]:
        cursor = self.cursor()

        cursor.execute(f"SELECT * FROM {self._NAME}")
        results = cursor.fetchall()

        return [models.DatabaseUser.model_validate(dict(result)) for result in results]
