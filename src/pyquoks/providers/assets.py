import io
import os

import PIL.Image
import requests

from .. import utils


class AssetsProvider(utils._HasRequiredAttributes):
    """
    Class for providing various assets data

    **Required attributes**::

        # Predefined

        _PATH = pyquoks.utils.get_path("assets/")

    Attributes:
        _PATH: Path to the directory with assets folders
    """

    _REQUIRED_ATTRIBUTES = {
        "_PATH",
    }

    _PATH: str = utils.get_path("assets/")

    def __init__(self) -> None:
        self._check_attributes()

        for attribute, object_type in self.__class__.__annotations__.items():
            if not issubclass(object_type, Directory | Network):
                continue

            setattr(self, attribute, object_type(self))


class Directory(utils._HasRequiredAttributes):
    """
    Class that represents a directory with various assets

    **Required attributes**::

        _PATH = "images/"

        _FILENAMES = {"picture1.png", "picture2.jpg"}

    Attributes:
        _PATH: Relative path to the directory with assets files
        _FILENAMES: Filenames of files in the directory
        _parent: Parent object
    """

    _REQUIRED_ATTRIBUTES = {
        "_PATH",
        "_FILENAMES",
    }

    _PATH: str

    _FILENAMES: set[str]

    _parent: AssetsProvider

    def __init__(self, parent: AssetsProvider) -> None:
        self._check_attributes()

        self._parent = parent

        for attribute in self._FILENAMES:
            try:
                setattr(self, os.path.splitext(attribute)[0], self.file_image(
                    path=os.path.join(self._parent._PATH, self._PATH, attribute),
                ))
            except Exception:
                setattr(self, attribute, None)

    @staticmethod
    def file_image(path: str) -> PIL.Image.Image:
        """
        :param path: Absolute path of the image file
        :return: Image object from a file
        """

        with open(path, "rb") as file:
            return PIL.Image.open(
                fp=io.BytesIO(file.read()),
            )


class Network(utils._HasRequiredAttributes):
    """
    Class that represents a set of images obtained from a network

    **Required attributes**::

        _URLS = {"example": "https://example.com/image.png"}

    Attributes:
        _URLS: Dictionary with names of attributes and URLs
        _parent: Parent object
    """

    _REQUIRED_ATTRIBUTES = {
        "_URLS",
    }

    _URLS: dict[str, str]

    _parent: AssetsProvider

    def __init__(self, parent: AssetsProvider) -> None:
        self._check_attributes()

        self._parent = parent

        for attribute, url in self._URLS.items():
            try:
                setattr(self, attribute, self.network_image(
                    url=url,
                ))
            except Exception:
                setattr(self, attribute, None)

    @staticmethod
    def network_image(url: str) -> PIL.Image.Image:
        """
        :param url: URL of the image file
        :return: Image object from a URL
        """

        return PIL.Image.open(
            fp=io.BytesIO(requests.get(url).content),
        )
