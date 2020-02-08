1. Action создает Message и отправляет его в ServiceBus.
2. ServiceBus достает сагу для этого типа Message и Message.CorrelationId из SagaKeeper.
   если Message.CorrelationId еще нету, или сага еще не запомнена, создает сагу.
3. ServiceBus вызывает обработчик саги для заданного типа сообщения
   и вызывает SagaUpdate, котрый присваивает CorrelationId и сохряняет сагу


	




TelegramSaga 

OnStart
  httpPost()
  bus.SendMessage({Correlation.Id = telegram.charId})


OnTelegramResponse() {
    process: resume
}





HttpServer
    TelegramListener
        OnResponse
            bus.SendMessage(new TelegramResponse(chatId))
    



EventBus
    _sagas.Add({typeof(Saga), correlation.Id}, saga)

    TMessage, TSaga
