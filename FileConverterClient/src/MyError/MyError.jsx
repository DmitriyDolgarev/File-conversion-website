function MyError(props) {
    return (
        <div className={` ${props.isError ? '' : 'invisible'}  bg-fc-light-gray mt-5 lg:ml-30 border-1 lg:w-140 border-fc-gray text-sm lg:text-base py-1.5 px-5 lg:px-10 rounded-lg text-fc-orange`}>
            {props.isTypeError ?
                'Извините, мы ещё не умеем  работать с таким типом файлов' 
                : 
                 props.isStringError
                ? 
                'Пожалуйста, корректно введите страницы для разбиения'
                :
                'Пожалуйста, выбирайте файлы с одинаковым расширением'
            }
        </div>
    )
}

export default MyError