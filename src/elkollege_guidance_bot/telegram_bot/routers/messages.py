import datetime
import typing

import aiogram
import aiogram.filters
import aiogram.fsm.context

from .. import states
from ..providers import keyboards
from ..services import logger
from ... import constants
from ... import models
from ... import utils
from ...managers import config
from ...managers import database
from ...providers import guidance
from ...providers import strings


class MessagesRouter(aiogram.Router):
    def __init__(
            self,
            config_manager: config.ConfigManager,
            database_manager: database.DatabaseManager,
            guidance_provider: guidance.GuidanceProvider,
            keyboards_provider: keyboards.KeyboardsProvider,
            strings_provider: strings.StringsProvider,
            logger_service: logger.LoggerService,
            aiogram_bot: aiogram.Bot,
    ) -> None:
        self._config = config_manager
        self._database = database_manager
        self._guidance = guidance_provider
        self._keyboards = keyboards_provider
        self._strings = strings_provider
        self._logger = logger_service
        self._bot = aiogram_bot

        super().__init__(
            name=self.__class__.__name__,
        )

        self.message.outer_middleware.register(
            self._outer_middleware,
        )
        # noinspection argument-list
        self.message.register(
            self._personal_data_handler,
            aiogram.filters.StateFilter(
                states.Flow.input_full_name,
                states.Flow.input_phone_number,
                states.Flow.input_email,
                states.Flow.input_institution,
                states.Flow.input_current_course,
            ),
        )

        self._logger.info(f"{self.name} initialized!")

    # region Middlewares

    async def _outer_middleware(
            self,
            handler: typing.Callable[
                [
                    aiogram.types.Message,
                    dict[str, typing.Any],
                ],
                typing.Awaitable[typing.Any],
            ],
            message: aiogram.types.Message,
            data: dict[str, typing.Any],
    ) -> typing.Any:
        if message.from_user is None:
            return None

        state = data["raw_state"]

        self._logger.log_telegram_user_interaction(
            user=message.from_user,
            interaction=f"{message.text} ({state=})",
        )

        return await handler(message, data)

    # endregion

    # region Handlers

    async def _personal_data_handler(
            self,
            message: aiogram.types.Message,
            state: aiogram.fsm.context.FSMContext,
    ) -> None:
        if message.text is None:
            return

        current_state = await state.get_state()

        match current_state:
            case states.Flow.input_full_name:
                await state.update_data(
                    data={
                        self._strings.state.current_full_name: message.text,
                    },
                )
                await state.set_state(states.Flow.input_phone_number)

                await self._bot.send_message(
                    chat_id=message.chat.id,
                    text=self._strings.menu.input_phone_number(),
                )
            case states.Flow.input_phone_number:
                if not utils.match_phone_number(message.text):
                    current_retries_count: int = await state.get_value(
                        key=self._strings.state.input_phone_number_retries_count,
                        default=constants.DEFAULT_MATCH_RETRIES_COUNT,
                    )

                    if current_retries_count < constants.MAX_MATCH_RETRIES_COUNT:
                        await state.update_data(
                            data={
                                self._strings.state.input_phone_number_retries_count: current_retries_count + 1,
                            },
                        )

                        await self._bot.send_message(
                            chat_id=message.chat.id,
                            text=self._strings.menu.input_phone_number_error(),
                        )
                        return

                    await state.update_data(
                        data={
                            self._strings.state.input_phone_number_retries_count: constants.DEFAULT_MATCH_RETRIES_COUNT,
                        },
                    )
                    return

                await state.update_data(
                    data={
                        self._strings.state.current_phone_number: message.text,
                    },
                )

                #     await state.set_state(states.Flow.input_email)
                #
                #     await self._bot.send_message(
                #         chat_id=message.chat.id,
                #         text=self._strings.menu.input_email(),
                #     )
                # case states.Flow.input_email:
                #     if not utils.match_email(message.text):
                #         current_retries_count: int = await state.get_value(
                #             key=self._strings.state.input_email_retries_count,
                #             default=constants.DEFAULT_MATCH_RETRIES_COUNT,
                #         )
                #
                #         if current_retries_count < constants.MAX_MATCH_RETRIES_COUNT:
                #             await state.update_data(
                #                 data={
                #                     self._strings.state.input_email_retries_count: current_retries_count + 1,
                #                 },
                #             )
                #
                #             await self._bot.send_message(
                #                 chat_id=message.chat.id,
                #                 text=self._strings.menu.input_email_error(),
                #             )
                #             return
                #
                #         await state.update_data(
                #             data={
                #                 self._strings.state.input_email_retries_count: constants.DEFAULT_MATCH_RETRIES_COUNT,
                #             },
                #         )
                #         return
                #
                #     await state.update_data(
                #         data={
                #             self._strings.state.current_email: message.text,
                #         },
                #     )

                current_block = self._guidance.test.blocks[0]

                await state.set_state(states.Flow.career_guidance_test)

                await self._bot.send_message(
                    chat_id=message.chat.id,
                    text=self._strings.menu.block(current_block),
                    reply_markup=self._keyboards.block(
                        block_index=0,
                    ),
                )
            case states.Flow.input_institution:
                current_user_type: models.UserType = await state.get_value(
                    key=self._strings.state.current_user_type,
                    default=models.UserType.SCHOOLKID,
                )

                await state.update_data(
                    data={
                        self._strings.state.current_institution: message.text,
                    },
                )

                if current_user_type == models.UserType.SCHOOLKID:
                    await self._send_possible_type(
                        message=message,
                        state=state,
                    )
                    return

                await state.set_state(states.Flow.input_current_course)

                await self._bot.send_message(
                    chat_id=message.chat.id,
                    text=self._strings.menu.input_current_course(),
                )
            case states.Flow.input_current_course:
                await state.update_data(
                    data={
                        self._strings.state.current_course: message.text,
                    },
                )

                await self._send_possible_type(
                    message=message,
                    state=state,
                )

    # endregion

    # region Helpers

    async def _send_possible_type(
            self,
            message: aiogram.types.Message,
            state: aiogram.fsm.context.FSMContext,
    ) -> None:
        current_types_rating: dict = await state.get_value(
            key=self._strings.state.types_rating,
            default={},
        )

        current_possible_type = self._guidance.get_possible_type(
            types_rating=current_types_rating,
        )
        current_timestamp = int(datetime.datetime.now().timestamp())

        current_user_type: models.UserType = await state.get_value(
            key=self._strings.state.current_user_type,
            default=models.UserType.SCHOOLKID,
        )
        current_full_name: str = await state.get_value(
            key=self._strings.state.current_full_name,
            default="",
        )
        current_phone_number: str | None = await state.get_value(
            key=self._strings.state.current_phone_number,
        )
        current_email: str | None = await state.get_value(
            key=self._strings.state.current_email,
        )
        current_institution: str = await state.get_value(
            key=self._strings.state.current_institution,
            default="",
        )
        current_course: str | None = await state.get_value(
            key=self._strings.state.current_course,
        )

        await state.clear()

        self._database.users.add_user(
            type_=current_user_type,
            full_name=current_full_name,
            phone_number=current_phone_number,
            email=current_email,
            institution=current_institution,
            current_course=current_course,
            possible_type=current_possible_type.name,
            timestamp=current_timestamp,
        )

        await self._bot.send_message(
            chat_id=message.chat.id,
            text=self._strings.menu.possible_type(
                possible_type=current_possible_type,
            ),
        )

    # endregion
