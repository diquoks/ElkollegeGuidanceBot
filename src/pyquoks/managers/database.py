import os
import sqlite3

from .. import utils


class DatabaseManager(utils._HasRequiredAttributes):
    """
    Class for managing database connections

    **Required attributes**::

        # Predefined

        _PATH = pyquoks.utils.get_path("db/")

    Attributes:
        _PATH: Path to the directory with databases
    """

    _REQUIRED_ATTRIBUTES = {
        "_PATH",
    }

    _PATH: str = utils.get_path("db/")

    def __init__(self) -> None:
        self._check_attributes()

        os.makedirs(
            name=self._PATH,
            exist_ok=True,
        )

        for attribute, object_type in self.__class__.__annotations__.items():
            if not issubclass(object_type, Database):
                continue

            setattr(self, attribute, object_type(self))

    def close_all(self) -> None:
        """
        Closes all database connections
        """

        for attribute, object_type in self.__class__.__annotations__.items():
            if not issubclass(object_type, Database):
                continue

            getattr(self, attribute).close()


class Database(sqlite3.Connection, utils._HasRequiredAttributes):
    """
    Class that represents a database connection

    **Required attributes**::

        _NAME = "users"

        _SQL = f\"""CREATE TABLE IF NOT EXISTS {_NAME} (user_id INTEGER PRIMARY KEY NOT NULL)\"""

    Attributes:
        _NAME: Name of the database
        _SQL: SQL expression for creating a table
        _parent: Parent object
    """

    _REQUIRED_ATTRIBUTES = {
        "_NAME",
        "_SQL",
    }

    _NAME: str

    _SQL: str

    _parent: DatabaseManager

    def __init__(self, parent: DatabaseManager) -> None:
        self._check_attributes()

        self._parent = parent

        super().__init__(
            database=os.path.join(self._parent._PATH, f"{self._NAME}.db"),
            check_same_thread=False,
        )
        self.row_factory = sqlite3.Row

        cursor = self.cursor()

        cursor.execute(self._SQL)

        self.commit()
