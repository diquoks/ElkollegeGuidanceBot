import typing

import aiogram
import aiogram.filters
import aiogram.fsm.context

import pyquoks.utils
from .. import states
from ..providers import keyboards
from ..services import logger
from ... import constants
from ... import models
from ...providers import guidance
from ...providers import strings


class CallbacksRouter(aiogram.Router):
    def __init__(
            self,
            guidance_provider: guidance.GuidanceProvider,
            keyboards_provider: keyboards.KeyboardsProvider,
            strings_provider: strings.StringsProvider,
            logger_service: logger.LoggerService,
            aiogram_bot: aiogram.Bot,
    ) -> None:
        self._guidance = guidance_provider
        self._keyboards = keyboards_provider
        self._strings = strings_provider
        self._logger = logger_service
        self._bot = aiogram_bot

        super().__init__(
            name=self.__class__.__name__,
        )

        self.callback_query.outer_middleware.register(
            self._outer_middleware,
        )
        self.callback_query.register(
            self._callback_handler,
        )

        self._logger.info(f"{self.name} initialized!")

    # region Middlewares

    async def _outer_middleware(
            self,
            handler: typing.Callable[
                [
                    aiogram.types.CallbackQuery,
                    dict[str, typing.Any],
                ],
                typing.Awaitable[typing.Any],
            ],
            call: aiogram.types.CallbackQuery,
            data: dict[str, typing.Any],
    ) -> typing.Any:
        if call.data is None:
            return None

        state = data["raw_state"]

        self._logger.log_telegram_user_interaction(
            user=call.from_user,
            interaction=f"{call.data} ({state=})",
        )

        return await handler(call, data)

    # endregion

    # region Handlers

    async def _callback_handler(
            self,
            call: aiogram.types.CallbackQuery,
            state: aiogram.fsm.context.FSMContext,
    ) -> None:
        if call.data is None or call.message is None:
            return

        current_state = await state.get_state()

        try:
            match call.data.split():
                case [
                    self._strings.callback.start,
                ]:
                    await state.clear()

                    await state.set_state(states.Flow.personal_data_agreement)

                    await self._bot.edit_message_media(
                        chat_id=call.message.chat.id,
                        message_id=call.message.message_id,
                        media=aiogram.types.InputMediaDocument(
                            media=aiogram.types.FSInputFile(
                                path=pyquoks.utils.get_path("assets/personal_data_agreement.docx"),
                                filename="Согласие.docx",
                            ),
                            caption=self._strings.menu.personal_data_agreement(),
                        ),
                        reply_markup=self._keyboards.personal_data_agreement(),
                    )
                case [
                    self._strings.callback.personal_data_agreement,
                    agreement_status,
                ] if current_state == states.Flow.personal_data_agreement:
                    current_agreement_status = bool(int(agreement_status))

                    if not current_agreement_status:
                        await state.clear()

                        await self._bot.delete_message(
                            chat_id=call.message.chat.id,
                            message_id=call.message.message_id,
                        )
                        await self._bot.send_message(
                            chat_id=call.message.chat.id,
                            text=self._strings.menu.personal_data_agreement_disagree(),
                        )
                        return

                    await state.set_state(states.Flow.input_full_name)

                    await self._bot.edit_message_reply_markup(
                        chat_id=call.message.chat.id,
                        message_id=call.message.message_id,
                    )
                    await self._bot.send_message(
                        chat_id=call.message.chat.id,
                        text=self._strings.menu.input_full_name(),
                    )
                case [
                    self._strings.callback.block,
                    block_index,
                ] if current_state == states.Flow.career_guidance_test:
                    current_block_index = int(block_index)

                    current_block = self._guidance.test.blocks[current_block_index]

                    match current_block.type:
                        case models.GuidanceBlockType.OPTIONS:
                            current_question: models.GuidanceOptionsQuestion = current_block.questions[0]

                            await self._bot.edit_message_text(
                                chat_id=call.message.chat.id,
                                message_id=call.message.message_id,
                                text=self._strings.menu.options_question(
                                    question=current_question,
                                ),
                                reply_markup=self._keyboards.question_options_answers(
                                    block_index=current_block_index,
                                    question_index=0,
                                    answers=current_question.answers,
                                ),
                            )
                        case models.GuidanceBlockType.BINARY:
                            current_question: models.GuidanceBinaryQuestion = current_block.questions[0]

                            await self._bot.edit_message_text(
                                chat_id=call.message.chat.id,
                                message_id=call.message.message_id,
                                text=self._strings.menu.binary_question(
                                    question=current_question,
                                ),
                                reply_markup=self._keyboards.question_binary_answers(
                                    block_index=current_block_index,
                                    question_index=0,
                                ),
                            )
                case [
                    self._strings.callback.answer,
                    block_index,
                    question_index,
                    answer_index,
                ] if current_state == states.Flow.career_guidance_test:
                    current_block_index = int(block_index)
                    current_question_index = int(question_index)
                    current_answer_index = int(answer_index)

                    current_types_rating: dict = await state.get_value(
                        key=self._strings.state.types_rating,
                        default={},
                    )

                    current_block = self._guidance.test.blocks[current_block_index]

                    match current_block.type:
                        case models.GuidanceBlockType.OPTIONS:
                            current_question: models.GuidanceOptionsQuestion = current_block.questions[
                                current_question_index
                            ]
                            current_answer = current_question.answers[current_answer_index]

                            current_type_rating = current_types_rating.get(current_answer.type_id, 0)
                            current_types_rating.update(
                                {
                                    current_answer.type_id: current_type_rating + 1,
                                }
                            )
                        case models.GuidanceBlockType.BINARY:
                            current_question: models.GuidanceBinaryQuestion = current_block.questions[
                                current_question_index
                            ]
                            current_answer = bool(current_answer_index)

                            current_type_rating = current_types_rating.get(current_question.type_id, 0)
                            current_types_rating.update(
                                {
                                    current_question.type_id: current_type_rating + int(current_answer),
                                }
                            )

                    await state.update_data(
                        data={
                            self._strings.state.types_rating: current_types_rating,
                        },
                    )

                    current_question_index += 1

                    if current_question_index < len(current_block.questions):
                        match current_block.type:
                            case models.GuidanceBlockType.OPTIONS:
                                current_question: models.GuidanceOptionsQuestion = current_block.questions[
                                    current_question_index
                                ]

                                await self._bot.edit_message_text(
                                    chat_id=call.message.chat.id,
                                    message_id=call.message.message_id,
                                    text=self._strings.menu.options_question(
                                        question=current_question,
                                    ),
                                    reply_markup=self._keyboards.question_options_answers(
                                        block_index=current_block_index,
                                        question_index=current_question_index,
                                        answers=current_question.answers,
                                    ),
                                )
                            case models.GuidanceBlockType.BINARY:
                                current_question: models.GuidanceBinaryQuestion = current_block.questions[
                                    current_question_index
                                ]

                                await self._bot.edit_message_text(
                                    chat_id=call.message.chat.id,
                                    message_id=call.message.message_id,
                                    text=self._strings.menu.binary_question(
                                        question=current_question,
                                    ),
                                    reply_markup=self._keyboards.question_binary_answers(
                                        block_index=current_block_index,
                                        question_index=current_question_index,
                                    ),
                                )
                        return

                    current_block_index += 1

                    if current_block_index < len(self._guidance.test.blocks):
                        current_block = self._guidance.test.blocks[current_block_index]

                        await self._bot.edit_message_text(
                            chat_id=call.message.chat.id,
                            message_id=call.message.message_id,
                            text=self._strings.menu.block(current_block),
                            reply_markup=self._keyboards.block(
                                block_index=current_block_index,
                            ),
                        )
                        return

                    await state.set_state(states.Flow.select_user_type)

                    await self._bot.edit_message_text(
                        chat_id=call.message.chat.id,
                        message_id=call.message.message_id,
                        text=self._strings.menu.user_type(),
                        reply_markup=self._keyboards.user_type(),
                    )
                case [
                    self._strings.callback.user_type,
                    user_type,
                ] if current_state == states.Flow.select_user_type:
                    current_user_type = models.UserType(int(user_type))

                    await state.update_data(
                        data={
                            self._strings.state.current_user_type: current_user_type,
                        },
                    )
                    await state.set_state(states.Flow.input_institution)

                    await self._bot.edit_message_text(
                        chat_id=call.message.chat.id,
                        message_id=call.message.message_id,
                        text=self._strings.menu.input_institution(),
                    )
                case _:
                    await self._bot.answer_callback_query(
                        callback_query_id=call.id,
                        text=self._strings.alert.button_unavailable(),
                        show_alert=True,
                    )
        except Exception as exception:
            if type(exception) in constants.AIOGRAM_IGNORED_EXCEPTIONS:
                return

            self._logger.log_exception(exception)
        finally:
            await self._bot.answer_callback_query(
                callback_query_id=call.id,
            )

    # endregion
