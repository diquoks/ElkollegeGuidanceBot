import os


class EnvironmentProvider:
    """
    Class for providing environment variables
    """

    def __init__(self) -> None:
        self.load_variables()

    def load_variables(self) -> None:
        """
        Loads specified environment variables
        """

        for attribute in self.__class__.__annotations__.keys():
            setattr(self, attribute, os.getenv(attribute, None))
