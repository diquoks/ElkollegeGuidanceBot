import datetime
import logging
import os
import sys

from .. import utils


class LoggerService(logging.Logger):
    """
    Class that provides methods for parallel logging
    """

    def __init__(
            self,
            name: str,
            level: int = logging.NOTSET,
            file_handling: bool = True,
            path: str = utils.get_path("logs/"),
    ) -> None:
        super().__init__(name, level)

        self.filename = f"{int(datetime.datetime.now().timestamp())}.{name}.log"
        self.encoding = "utf-8"
        self._path = os.path.join(path, self.filename)

        def new_formatter(fmt: str) -> logging.Formatter:
            return logging.Formatter(
                fmt=fmt,
                datefmt="%d-%m-%y %H:%M:%S",
                style="$",
            )

        self._stdout_handler = logging.StreamHandler(sys.stdout)
        self._stdout_handler.setFormatter(new_formatter("$levelname $asctime $name - $message"))
        self.addHandler(self._stdout_handler)

        if not file_handling:
            self._file_handler = None
            return

        os.makedirs(
            name=path,
            exist_ok=True,
        )

        self._file_handler = logging.FileHandler(
            filename=self._path,
            encoding=self.encoding,
        )
        self._file_handler.setFormatter(new_formatter("$levelname $asctime - $message"))
        self.addHandler(self._file_handler)

    def read(self) -> str | None:
        if self._file_handler is None:
            return None

        with open(self._path, "r", encoding=self.encoding) as file:
            return file.read()

    def log_exception(self, exception: Exception, raise_again: bool = False) -> None:
        """
        Logs an exception with detailed traceback

        :param exception: Exception to be logged
        :param raise_again: Whether exception should be raised again
        """

        self.error(
            msg=exception,
            exc_info=True,
        )

        if raise_again:
            raise exception
