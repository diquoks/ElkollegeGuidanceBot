from .. import models


class StringsProvider:
    alert: AlertStrings
    button: ButtonStrings
    callback: CallbackStrings
    menu: MenuStrings
    state: StateStrings

    def __init__(self) -> None:
        self.alert = AlertStrings()
        self.button = ButtonStrings()
        self.callback = CallbackStrings()
        self.menu = MenuStrings()
        self.state = StateStrings()


class AlertStrings:

    @classmethod
    def button_unavailable(cls) -> str:
        return "Данное тестирование недоступно, начни новое!"


class ButtonStrings:

    @classmethod
    def start(cls) -> str:
        return "Начать тестирование"

    @classmethod
    def yes(cls) -> str:
        return "Да"

    @classmethod
    def no(cls) -> str:
        return "Нет"

    @classmethod
    def continue_(cls) -> str:
        return "Продолжить"


class CallbackStrings:

    @property
    def start(self) -> str:
        return "start"

    @property
    def user_type(self) -> str:
        return "user_type"

    @property
    def personal_data_agreement(self) -> str:
        return "personal_data_agreement"

    @property
    def block(self) -> str:
        return "block"

    @property
    def answer(self) -> str:
        return "answer"


class MenuStrings:

    # region /start

    @classmethod
    def start(cls, bot_name: str) -> str:
        return f"""\
<b>Добро пожаловать в {bot_name}!</b>

Здесь ты сможешь пройти профориентационное тестирование и узнать, какие профессии подходят тебе больше всего.

В опросе нет правильных или неправильных ответов, выбирай тот вариант, который больше всего похож на тебя."""

    @classmethod
    def start_has_active_test(cls) -> str:
        return "У тебя уже есть активное тестирование, хочешь начать новое?"

    @classmethod
    def personal_data_agreement(cls) -> str:
        return """\
Тестирование не является диагностическим инструментом и не даёт окончательного ответа.
Его цель — запустить размышление и помочь увидеть свои склонности.

Продолжая, ты соглашаешься с политикой обработки персональных данных."""

    @classmethod
    def personal_data_agreement_disagree(cls) -> str:
        return "Спасибо за уделённое время!"

    @classmethod
    def input_full_name(cls) -> str:
        return "Введи своё полное ФИО:"

    @classmethod
    def input_phone_number(cls) -> str:
        return "Введи свой телефонный номер:"

    @classmethod
    def input_phone_number_error(cls) -> str:
        return f"""\
<b>Некорректный формат!</b>
Попробуй ввести телефонный номер в следующем формате:
<pre>+7(987)654-32-10</pre>

{cls.input_phone_number()}"""

    @classmethod
    def input_email(cls) -> str:
        return "Введи свой адрес электронной почты:"

    @classmethod
    def input_email_error(cls) -> str:
        return f"""\
<b>Некорректный формат!</b>
Попробуй ввести адрес электронной почты в следующем формате:
<pre>email@example.com</pre>

{cls.input_email()}"""

    @classmethod
    def block(cls, block: models.GuidanceBlock) -> str:
        return f"""\
<b>Блок {block.id} | {block.title}</b>
{block.hint}"""

    @classmethod
    def options_question(cls, question: models.GuidanceOptionsQuestion) -> str:
        return f"""\
<b>{question.question}</b>

{"\n".join(f"{index}. {answer.answer}" for index, answer in enumerate(question.answers, start=1))}"""

    @classmethod
    def binary_question(cls, question: models.GuidanceBinaryQuestion) -> str:
        return f"<b>{question.question}</b>"

    @classmethod
    def user_type(cls) -> str:
        return "Выбери свой текущий статус:"

    @classmethod
    def input_institution(cls) -> str:
        return "Введи название своего текущего учебного заведения:"

    @classmethod
    def input_current_course(cls) -> str:
        return "Введи направление на котором сейчас обучаешься:"

    @classmethod
    def possible_type(cls, possible_type: models.GuidanceType) -> str:
        return f"""\
<b>Тестирование окончено!</b>
Твой тип: {possible_type.name} ({possible_type.class_} тип)

Подходящие профессии: {", ".join(possible_type.professions)}."""

    # endregion

    # region /export

    @classmethod
    def export_unavailable(cls) -> str:
        return "Экспорт недоступен!"

    # endregion

    # region /logs

    @classmethod
    def logging_disabled(cls) -> str:
        return "Логирование отключено!"

    # endregion


class StateStrings:
    @property
    def current_user_type(self) -> str:
        return "current_user_type"

    @property
    def current_full_name(self) -> str:
        return "current_full_name"

    @property
    def current_phone_number(self) -> str:
        return "current_phone_number"

    @property
    def current_email(self) -> str:
        return "current_email"

    @property
    def current_institution(self) -> str:
        return "current_institution"

    @property
    def current_course(self) -> str:
        return "current_course"

    @property
    def input_phone_number_retries_count(self) -> str:
        return "input_phone_number_retries_count"

    @property
    def input_email_retries_count(self) -> str:
        return "input_email_retries_count"

    @property
    def types_rating(self) -> str:
        return "types_rating"
