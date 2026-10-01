import aiogram

from ... import models
from ...providers import strings


class ButtonsProvider:
    def __init__(
            self,
            strings_provider: strings.StringsProvider,
    ) -> None:
        self._strings = strings_provider

    # region /start

    def start(self) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.start(),
            callback_data=self._strings.callback.start,
        )

    def agree(self) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.yes(),
            callback_data=f"{self._strings.callback.personal_data_agreement} 1",
        )

    def disagree(self) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.no(),
            callback_data=f"{self._strings.callback.personal_data_agreement} 0",
        )

    def block(self, block_index: int) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.continue_(),
            callback_data=f"{self._strings.callback.block} {block_index}",
        )

    def answer(
            self,
            block_index: int,
            question_index: int,
            answer_index: int,
    ) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=f"{answer_index + 1}",
            callback_data=f"{self._strings.callback.answer} {block_index} {question_index} {answer_index}",
        )

    def answer_yes(
            self,
            block_index: int,
            question_index: int,
    ) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.yes(),
            callback_data=f"{self._strings.callback.answer} {block_index} {question_index} 1",
        )

    def answer_no(
            self,
            block_index: int,
            question_index: int,
    ) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.no(),
            callback_data=f"{self._strings.callback.answer} {block_index} {question_index} 0",
        )

    def user_type(self, user_type: models.UserType) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=user_type.readable_name,
            callback_data=f"{self._strings.callback.user_type} {user_type.value}",
        )

    # endregion
