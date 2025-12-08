namespace Order.Api.Errors;

internal static class ErrorCatalog
{
    internal static class Orders
    {
        // CreateOrder
        public const string CreateInternalError   = "001X001 - Falha interna no servidor ao criar pedido";
        public const string CreateCancelled       = "001X002 - Requisição cancelada ao criar pedido";

        // AddItem
        public const string AddItemValidation     = "004V001 - Erros de validação ao adicionar item ao pedido";
        public const string AddItemInternalError  = "004X001 - Falha interna no servidor ao adicionar item ao pedido";
        public const string AddItemCancelled      = "004X002 - Requisição cancelada ao adicionar item ao pedido";

        // GetOrder
        public const string GetOrderNotFound      = "003X001 - Pedido não encontrado";
        public const string GetOrderInternalError = "003X002 - Falha interna ao buscar pedido";
        public const string GetOrderCancelled     = "003X003 - Requisição cancelada ao buscar pedido";

        // GetAllOrders
        public const string GetAllInternalError   = "005X001 - Falha interna ao listar pedidos";
        public const string GetAllCancelled       = "005X002 - Requisição cancelada ao listar pedidos";

        // CloseOrder
        public const string CloseValidation       = "002V001 - Erros de validação ao fechar pedido";
        public const string CloseNotFound         = "002N001 - Pedido não encontrado para fechamento";
        public const string CloseBusinessRule     = "002B001 - Pedido não pode ser fechado.";
        public const string CloseCancelled        = "002X002 - Requisição cancelada ao fechar pedido";
        public const string CloseInternalError = "002X001 - Falha interna no servidor ao fechar o pedido";
        
        // RemoveItem
        public const string RemoveItemValidation   = "006V001 - Erros de validação ao remover item do pedido";
        public const string RemoveItemNotFound     = "006N001 - Item ou pedido não encontrado para remoção";
        public const string RemoveItemBusinessRule = "006B001 - Item não pode ser removido do pedido.";
        public const string RemoveItemCancelled    = "006X002 - Requisição cancelada ao remover item do pedido";
        public const string RemoveItemInternalError= "006X001 - Falha interna no servidor ao remover item do pedido";
    }
}
