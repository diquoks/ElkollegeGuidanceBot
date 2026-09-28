import datetime
import os
import platform
import subprocess
import sys
import textwrap

import psutil


def format_multiline_string(string: str, *args, **kwargs) -> str:
    """
    :param string: Multiline string to format
    :param args: Arguments for formatting
    :param kwargs: Keyword arguments for formatting
    :return: Formatted multiline string
    """

    return textwrap.dedent(string).strip().format(*args, **kwargs)


def check_connection() -> bool:
    """
    :return: Status of "ping www.google.com"
    """

    return subprocess.run(
        args=[
            "ping",
            "-n" if platform.system().lower() == "windows" else "-c",
            "1",
            "www.google.com",
        ],
        capture_output=True,
    ).returncode == 0


def get_path(relative_path: str, use_meipass: bool = False) -> str:
    """
    :param relative_path: Relative path of the file
    :param use_meipass: Whether ``sys._MEIPASS`` should be used
    :return: Absolute path for provided relative path
    """

    if not use_meipass:
        base_path = os.path.abspath(".")
    elif use_meipass and hasattr(sys, "_MEIPASS"):
        base_path = sys._MEIPASS
    else:
        raise AttributeError(f"sys._MEIPASS is not specified!")

    return os.path.join(base_path, relative_path)


def get_process_created_datetime(pid: int | None = None) -> datetime.datetime:
    """
    :param pid: ID of the process
    :return: Datetime when the process was created
    """

    process = psutil.Process(pid or os.getpid())

    return datetime.datetime.fromtimestamp(
        timestamp=process.create_time(),
    )


class _HasRequiredAttributes:
    """
    Assistive class for checking required attributes

    **Required attributes**::

        _REQUIRED_ATTRIBUTES = {"_ATTRIBUTES", "_PATH"}

    Attributes:
        _REQUIRED_ATTRIBUTES: Set of required attributes in the class
    """

    _REQUIRED_ATTRIBUTES: set[str]

    def _check_attributes(self) -> None:
        if not hasattr(self, "_REQUIRED_ATTRIBUTES"):
            return

        for attribute in self._REQUIRED_ATTRIBUTES:
            if hasattr(self, attribute):
                continue

            raise AttributeError(f"The required class attribute is not set! ({attribute})")
