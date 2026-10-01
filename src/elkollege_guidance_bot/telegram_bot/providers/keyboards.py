import aiogram
import aiogram.utils.keyboard

from . import buttons
from ... import models


class KeyboardsProvider:
    def __init__(
            self,
            buttons_provider: buttons.ButtonsProvider,
    ) -> None:
        self._buttons = buttons_provider

    # region /start

    def start(self) -> aiogram.types.InlineKeyboardMarkup:
        markup_builder = aiogram.utils.keyboard.InlineKeyboardBuilder()
        markup_builder.row(
            self._buttons.start(),
        )

        return markup_builder.as_markup()

    def personal_data_agreement(self) -> aiogram.types.InlineKeyboardMarkup:
        markup_builder = aiogram.utils.keyboard.InlineKeyboardBuilder()
        markup_builder.row(
            self._buttons.agree(),
            self._buttons.disagree(),
        )

        return markup_builder.as_markup()

    def block(self, block_index: int) -> aiogram.types.InlineKeyboardMarkup:
        markup_builder = aiogram.utils.keyboard.InlineKeyboardBuilder()
        markup_builder.row(
            self._buttons.block(
                block_index=block_index,
            ),
        )

        return markup_builder.as_markup()

    def question_options_answers(
            self,
            block_index: int,
            question_index: int,
            answers: list[models.GuidanceOptionsAnswer],
    ) -> aiogram.types.InlineKeyboardMarkup:
        markup_builder = aiogram.utils.keyboard.InlineKeyboardBuilder()
        markup_builder.row(
            *[
                self._buttons.answer(
                    block_index=block_index,
                    question_index=question_index,
                    answer_index=index,
                ) for index, answer in enumerate(answers)
            ],
        )

        return markup_builder.as_markup()

    def question_binary_answers(
            self,
            block_index: int,
            question_index: int,
    ) -> aiogram.types.InlineKeyboardMarkup:
        markup_builder = aiogram.utils.keyboard.InlineKeyboardBuilder()
        markup_builder.row(
            *[
                self._buttons.answer_yes(
                    block_index=block_index,
                    question_index=question_index,
                ),
                self._buttons.answer_no(
                    block_index=block_index,
                    question_index=question_index,
                ),
            ],
        )

        return markup_builder.as_markup()

    def user_type(self) -> aiogram.types.InlineKeyboardMarkup:
        markup_builder = aiogram.utils.keyboard.InlineKeyboardBuilder()
        markup_builder.row(
            self._buttons.user_type(
                user_type=models.UserType.SCHOOLKID,
            ),
        )
        markup_builder.row(
            self._buttons.user_type(
                user_type=models.UserType.COLLEGE_STUDENT,
            ),
            self._buttons.user_type(
                user_type=models.UserType.UNIVERSITY_STUDENT,
            ),
        )

        return markup_builder.as_markup()

    # endregion
