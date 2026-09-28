import configparser
import json
import typing

from .. import utils


class ConfigManager(utils._HasRequiredAttributes):
    """
    Class for managing data in configuration file

    **Required attributes**::

        # Predefined

        _PATH = pyquoks.utils.get_path("config.ini")

    Attributes:
        _PATH: Path to the configuration file
    """

    _REQUIRED_ATTRIBUTES = {
        "_PATH",
    }

    _PATH: str = utils.get_path("config.ini")

    def __init__(self) -> None:
        self._check_attributes()

        for attribute, object_type in self.__class__.__annotations__.items():
            if not issubclass(object_type, Config):
                continue

            setattr(self, attribute, object_type(self))


class Config(utils._HasRequiredAttributes):
    """
    Class that represents a section in configuration file

    **Required attributes**::

        _SECTION = "Settings"

    Attributes:
        _SECTION: Name of the section in configuration file
        _parent: Parent object
    """

    _REQUIRED_ATTRIBUTES = {
        "_SECTION",
    }

    _SECTION: str

    _parent: ConfigManager

    def __init__(self, parent: ConfigManager) -> None:
        self._check_attributes()

        self._parent = parent

        self._config = configparser.ConfigParser()
        self._config.read(self._parent._PATH)

        if not self._config.has_section(self._SECTION):
            self._config.add_section(self._SECTION)

        for attribute, object_type in self.__class__.__annotations__.items():
            try:
                setattr(self, attribute, self._config.get(self._SECTION, attribute))
            except Exception:
                self._config.set(self._SECTION, attribute, object_type.__name__)
                with open(self._parent._PATH, "w", encoding="utf-8") as file:
                    self._config.write(file)

        for attribute, object_type in self.__class__.__annotations__.items():
            try:
                match object_type():
                    case bool():
                        if getattr(self, attribute) not in [str(True), str(False)]:
                            setattr(self, attribute, None)
                            raise ValueError()

                        setattr(self, attribute, getattr(self, attribute) == str(True))
                    case int():
                        setattr(self, attribute, int(getattr(self, attribute)))
                    case float():
                        setattr(self, attribute, float(getattr(self, attribute)))
                    case str():
                        pass
                    case dict() | list():
                        setattr(self, attribute, json.loads(getattr(self, attribute)))
                    case _:
                        raise ValueError(f"{object_type.__name__} type is not supported!")
            except Exception:
                setattr(self, attribute, None)

                raise configparser.ParsingError("configuration file is filled incorrectly!")

    @property
    def values(self) -> dict | None:
        """
        :return: Values stored in this section
        """

        try:
            return {attribute: getattr(self, attribute) for attribute in self.__class__.__annotations__.keys()}
        except Exception:
            return None

    def update(self, **kwargs) -> None:
        """
        Updates provided attributes in object
        """

        for attribute, value in kwargs.items():
            if attribute not in self.__class__.__annotations__.keys():
                raise AttributeError(f"{attribute} is not specified!")

            object_type = self.__class__.__annotations__.get(attribute)

            # noinspection PyTypeChecker
            if not isinstance(value, typing.get_origin(object_type) or object_type):
                raise AttributeError(f"{attribute} has incorrect type! (must be {object_type.__name__})")

            setattr(self, attribute, value)

            try:
                # noinspection calling-non-callable
                match object_type():
                    case bool() | int() | float() | str():
                        self._config.set(self._SECTION, attribute, str(value))
                    case dict() | list():
                        self._config.set(self._SECTION, attribute, json.dumps(value))
                    case _:
                        raise ValueError()
            except:
                raise ValueError(f"{object_type.__name__} type is not supported!")

            with open(self._parent._PATH, "w", encoding="utf-8") as file:
                self._config.write(file)
