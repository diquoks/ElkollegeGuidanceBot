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

    def start_new_test(self) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.start_new_test(),
            callback_data=self._strings.callback.start,
        )

    def schoolkid(self) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.schoolkid(),
            callback_data=f"{self._strings.callback.user_type} {models.UserType.SCHOOLKID.value}",
        )

    def college_student(self) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.college_student(),
            callback_data=f"{self._strings.callback.user_type} {models.UserType.COLLEGE_STUDENT.value}",
        )

    def university_student(self) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.university_student(),
            callback_data=f"{self._strings.callback.user_type} {models.UserType.UNIVERSITY_STUDENT.value}",
        )

    def agree(self) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.agree(),
            callback_data=f"{self._strings.callback.personal_data_agreement} 1",
        )

    def disagree(self) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.disagree(),
            callback_data=f"{self._strings.callback.personal_data_agreement} 0",
        )

    def answer(self, question_index: int, answer_index: int) -> aiogram.types.InlineKeyboardButton:
        return aiogram.types.InlineKeyboardButton(
            text=self._strings.button.answer(
                answer_index=answer_index,
            ),
            callback_data=f"{self._strings.callback.answer} {question_index} {answer_index}",
        )

    # endregion
