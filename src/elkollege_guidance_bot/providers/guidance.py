import random

import yaml

import pyquoks.utils
from .. import models


class GuidanceProvider:
    test: models.GuidanceTest

    def __init__(self) -> None:
        with open(pyquoks.utils.get_path("assets/career_guidance_test.yaml"), "rb") as file:
            self.test = models.GuidanceTest.model_validate(yaml.load(file, yaml.FullLoader))

    def get_possible_type(self, types_rating: dict[int, int]) -> models.GuidanceType:
        max_rating = max(types_rating.values())

        possible_type_id = random.choice([
            types_id for types_id, rating in types_rating.items() if rating == max_rating
        ])

        return [type_ for type_ in self.test.types if type_.id == possible_type_id][0]
