Datos importantes:

AspNetRoleClaims: Almacena los claims asociados a cada rol en tu aplicación. Los claims son afirmaciones sobre el usuario, como permisos o roles.
AspNetRoles: Almacena los roles de usuario en tu aplicación. Los roles agrupan usuarios que comparten permisos o responsabilidades comunes.
AspNetUserClaims: Almacena los claims asociados a cada usuario en tu aplicación. Estos claims pueden contener información adicional sobre el usuario, como su nombre, correo electrónico, etc.
AspNetUserLogins: Utilizada para almacenar información sobre los proveedores de inicio de sesión externos asociados a cada usuario. Por ejemplo, si un usuario inicia sesión mediante Google, se almacenaría aquí la información relacionada con esa autenticación externa.
AspNetUserRoles: Tabla de relación muchos a muchos que relaciona usuarios con roles. Indica qué roles tiene asignados cada usuario.
AspNetUsers: Almacena información básica sobre los usuarios de tu aplicación, como su nombre de usuario, correo electrónico, contraseña (o referencia a un proveedor externo), etc.
AspNetUserTokens: Utilizada para almacenar tokens de autenticación para cada usuario. Por ejemplo, tokens de restablecimiento de contraseña, tokens de autenticación de dos factores, etc.
AspNetDeviceCodes: Utilizada en OAuth 2.0 para almacenar códigos de dispositivo para el flujo de autorización del dispositivo.
Keys: Utilizada para almacenar claves de seguridad, como claves de autenticación de dos factores.
PersistedGrants: Utilizada para almacenar tokens OAuth 2.0 y datos relacionados con la autorización persistente.

Cada usuario puede tener varias solicitudes de usuario.
Cada usuario puede tener varios inicios de sesión de usuario.
Cada usuario puede tener varios tokens de usuario.
Cada rol puede tener varias solicitudes de rol asociadas.
Cada usuario puede tener varios roles asociados, y cada rol puede estar asociado a varios usuarios. Esta es una relación de muchos a muchos que requiere una tabla de unión en la base de datos. La tabla de unión está representada por la entidad UserRole.


Explicación de ClockSkew
Por defecto, el TokenValidationParameters.ClockSkew está configurado con un valor de 5 minutos para permitir una pequeña discrepancia
en el tiempo entre el emisor del token y el validador. Esto es útil en entornos de producción donde puede haber ligeras diferencias
en el tiempo de los servidores. Sin embargo, para pruebas locales y ciertos escenarios específicos, es útil establecerlo en TimeSpan.Zero 
para asegurar que la expiración del token sea exacta.


Uso de BackgroundJob.Enqueue
Propósito: BackgroundJob.Enqueue se utiliza para ejecutar una tarea en segundo plano de forma asíncrona, fuera del hilo principal de la aplicación. Esto es adecuado para tareas que pueden llevar mucho tiempo y no necesitan completarse de inmediato.
Ejemplo de uso: Enviar correos electrónicos, procesar archivos, realizar cálculos extensivos.
Persistencia y reintentos: Los trabajos encolados con BackgroundJob.Enqueue se persisten en una base de datos (o algún tipo de almacenamiento) y Hangfire maneja automáticamente los reintentos en caso de fallos.
No bloquea el hilo principal: Permite que el flujo de la aplicación continúe sin esperar a que la tarea se complete.
Escalabilidad: Puede distribuir trabajos entre varios servidores y manejar cargas de trabajo altas.

Uso de await
Propósito: await se utiliza para esperar de forma asíncrona la finalización de una tarea dentro del mismo contexto o hilo. Permite que el hilo se libere mientras espera, pero continúa dentro del mismo flujo de ejecución.
Ejemplo de uso: Esperar una respuesta de una API, leer archivos, realizar operaciones de base de datos.
Contexto de ejecución: await mantiene el contexto de sincronización, lo que significa que después de que la tarea se completa, el código continúa ejecutándose en el mismo hilo.
No hay persistencia o reintentos automáticos: A diferencia de BackgroundJob.Enqueue, await no maneja la persistencia o reintentos en caso de fallos. Si se necesita, debe implementarse manualmente.
Bloqueo del flujo de la aplicación: Aunque no bloquea el hilo, await espera a que la tarea termine antes de continuar con la ejecución del código posterior.

Diferencias Clave
Contexto de ejecución: await mantiene el contexto de sincronización y continua en el mismo hilo, mientras que BackgroundJob.Enqueue ejecuta la tarea en un contexto completamente separado.
Persistencia y manejo de fallos: Hangfire maneja la persistencia y los reintentos automáticos de tareas fallidas, mientras que await no lo hace.
Idoneidad: BackgroundJob.Enqueue es más adecuado para tareas largas y no críticas que pueden ejecutarse independientemente del flujo principal de la aplicación. await es adecuado para tareas que deben completarse antes de continuar, manteniendo el flujo lógico del programa.





Colecciones:

Usar la interfaz más genérica posible:

public IEnumerable<string> GetNames()
{
    return new List<string> { "Alice", "Bob", "Charlie" 
};


Preferir IEnumerable<T> para iteración:

Si el propósito del método es simplemente iterar sobre una colección, usa IEnumerable<T>.
Por qué: Es la interfaz más genérica y cubre la mayoría de los casos de uso para la iteración.


Usar ICollection<T> para manipulación de colecciones:


// Para iteración
public IEnumerable<string> GetNames()
{
    return new List<string> { "Alice", "Bob", "Charlie" };
}

// Para manipulación básica
public ICollection<string> GetEditableNames()
{
    return new List<string> { "Alice", "Bob", "Charlie" };
}

// Para acceso por índice
public IList<string> GetIndexedNames()
{
    return new List<string> { "Alice", "Bob", "Charlie" };
}



Mapeos:
Veamos el siguiente ejemplo 

Esto es mucho más rapido que un mapeo. Ver código fuente con explicación
 Expression<Func<Domain.Entities.Pedidos, PedidosResponse>> expression = e => new PedidosResponse
            {
                Descripcion = e.Descripcion,
                Moneda = e.Moneda,
                Monto = e.Monto,
            };

            var filtro = new PedidosFilter(request.SearchString);
            if (request.OrderBy?.Any() != true)
            {
                var response = await _unitOfWork.Repository<Domain.Entities.Pedidos>().Entities
                    .Specify(filtro)
                    .Select(expression)
                    .ToPaginatedListAsync(request.PageNumber, request.PageSize);
                return response;
            }