function Error(props) {
    return (
        <div className={` ${props.isError ? '' : 'invisible'}  bg-fc-light-gray mt-5 ml-5 lg:ml-40 border-1 border-fc-gray text-sm lg:text-base py-1.5 lg:px-20 rounded-lg text-fc-orange`}>
            {props.isTypeError ?
                'Извините, мы ещё не умеем  работать с таким типом файлов' :
                'Пожалуйста, выбирайте файлы с одинаковым расширением'
            }
        </div>
    )
}

export default Error