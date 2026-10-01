import enum
import typing

import pydantic


class UserType(enum.IntEnum):
    SCHOOLKID = 0
    COLLEGE_STUDENT = 1
    UNIVERSITY_STUDENT = 2

    @property
    def readable_name(self) -> str:
        match self:
            case UserType.SCHOOLKID:
                return "Школьник"
            case UserType.COLLEGE_STUDENT:
                return "Студент СПО"
            case UserType.UNIVERSITY_STUDENT:
                return "Студент вуза"


class DatabaseUser(pydantic.BaseModel):
    id: int
    type: UserType
    full_name: str
    phone_number: str | None
    email: str | None
    institution: str
    current_course: str | None
    possible_type: str
    timestamp: int


class GuidanceTest(pydantic.BaseModel):
    types: list[GuidanceType]
    blocks: list[
        typing.Annotated[
            typing.Union[GuidanceOptionsBlock, GuidanceBinaryBlock],
            pydantic.Field(discriminator="type"),
        ],
    ]


class GuidanceType(pydantic.BaseModel):
    id: int
    name: str
    class_: typing.Annotated[
        str,
        pydantic.Field(alias="class"),
    ]
    professions: list[str]


class GuidanceBlockType(enum.IntEnum):
    OPTIONS = 1
    BINARY = 2


class GuidanceBlock(pydantic.BaseModel):
    id: int
    title: str
    type: typing.Literal[GuidanceBlockType.OPTIONS, GuidanceBlockType.BINARY]
    hint: str
    questions: list[typing.Any]


class GuidanceOptionsBlock(GuidanceBlock):
    type: typing.Literal[GuidanceBlockType.OPTIONS] = GuidanceBlockType.OPTIONS
    questions: list[GuidanceOptionsQuestion]


class GuidanceOptionsQuestion(pydantic.BaseModel):
    question: str
    answers: list[GuidanceOptionsAnswer]


class GuidanceOptionsAnswer(pydantic.BaseModel):
    answer: str
    type_id: int


class GuidanceBinaryBlock(GuidanceBlock):
    type: typing.Literal[GuidanceBlockType.BINARY] = GuidanceBlockType.BINARY
    questions: list[GuidanceBinaryQuestion]


class GuidanceBinaryQuestion(pydantic.BaseModel):
    question: str
    type_id: int
