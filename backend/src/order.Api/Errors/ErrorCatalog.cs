namespace Order.Api.Errors;

public static class ErrorCatalog
{
    public static class Orders
    {
        // CreateOrder
        public const string Create_InternalError   = "001X001 - Falha interna no servidor ao criar pedido";
        public const string Create_Cancelled       = "001X002 - Requisição cancelada ao criar pedido";

        // AddItem
        public const string AddItem_Validation     = "004V001 - Erros de validação ao adicionar item ao pedido";
        public const string AddItem_InternalError  = "004X001 - Falha interna no servidor ao adicionar item ao pedido";
        public const string AddItem_Cancelled      = "004X002 - Requisição cancelada ao adicionar item ao pedido";

        // GetOrder
        public const string GetOrder_NotFound      = "003X001 - Pedido não encontrado";
        public const string GetOrder_InternalError = "003X002 - Falha interna ao buscar pedido";
        public const string GetOrder_Cancelled     = "003X003 - Requisição cancelada ao buscar pedido";

        // GetAllOrders
        public const string GetAll_InternalError   = "005X001 - Falha interna ao listar pedidos";
        public const string GetAll_Cancelled       = "005X002 - Requisição cancelada ao listar pedidos";

        // CloseOrder
        public const string Close_Validation       = "002V001 - Erros de validação ao fechar pedido";
        public const string Close_NotFound         = "002N001 - Pedido não encontrado para fechamento";
        public const string Close_BusinessRule     = "002B001 - Pedido não pode ser fechado.";
        public const string Close_Cancelled        = "002X002 - Requisição cancelada ao fechar pedido";
        public const string Close_InternalError    = "002X001 - Falha interna no servidor ao fechar o pedido";
    }
}
